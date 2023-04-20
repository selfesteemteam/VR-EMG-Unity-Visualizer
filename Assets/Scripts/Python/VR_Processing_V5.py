# ------------------------------------------------------------------------------------
# Setup Process for IMU/EMG Sensors:                                                 |
# ------------------------------------------------------------------------------------
# EMG Sensors Face Setup   : Left Electrode = Red   and    Right Electrode = Blue    |
# EMG Cables on Cyton Board: Top Pin = Red          and    Bottom Pin = Blue         |
# -----------------------------------------------------------------------------------|
# EMG Stream 1 - N1P Channel on Cyton Board --- Top Left Above Eye --- Array#0-124   |
# EMG Stream 2 - N2P Channel on Cyton Board -- Top Right Above Eye --- Array#125-249 |
# EMG Stream 3 - N3P Channel on Cyton Board - Bottom Left Below Eye -- Array#250-374 |
# EMG Stream 4 - N4P Channel on Cyton Board - Bottom Right Below Eye - Array#375-499 |
# MMRL 1 ----------- CF:79:F7:71:EC:59 --------- Left Face Cheek ----- Array#500-509 |
# MMRL 2 ----------- D2:34:22:97:6B:E9 --------- Right Face Cheek ---- Array#510-519 |
# ------------------------------------------------------------------------------------

from __future__ import print_function
from asyncio.windows_events import NULL
#1from msilib.schema import Class

# Library for MMRL IMU Sensors
from mbientlab.metawear import MetaWear, libmetawear, parse_value
from mbientlab.metawear.cbindings import *

# Libraries for time management and delayed actions
import keyboard
from time import sleep
import time
from collections import deque

# Library for creating Directory paths
import os

# Library for saving data to .csv files
import csv

# Library for resolving and connecting to LSL EMG stream
from pylsl import StreamInlet, resolve_stream

# Libraries for ConvNet Training
import numpy as np
import pandas as pd
import tensorflow as tf
#from tensorflow import keras
#from tensorflow.keras.models import Sequential
#from tensorflow.keras.layers import Dense, Dropout, Activation, Flatten, LSTM, SimpleRNN, Embedding, Reshape
#from tensorflow.keras.layers import Conv1D, MaxPooling1D, BatchNormalization
import pickle
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from sklearn import svm, datasets
from sklearn.inspection import DecisionBoundaryDisplay
from sklearn.metrics import ConfusionMatrixDisplay
import matplotlib.pyplot as plt
from matplotlib import style

# Resampling dependencies
from scipy import signal
import time
import random
random.seed

# Libraries for UDP Unity Server
from UDPBroadcaster import UDPBroadcaster
import struct
import sys

# Class for storing and encoding a set of three floats
class ThreeTuple:
    def __init__(self, x, y, z):
        self.x = x
        self.y = y
        self.z = z

    def encode_as_floats(self):
        bytes =  bytearray(struct.pack("f", self.x))
        bytes += bytearray(struct.pack("f", self.y))
        bytes += bytearray(struct.pack("f", self.z))
        return bytes
    
class IMUState:
    # Initialize State for each MetaMotionRL used in the project (2)
    def __init__(self, device):
        self.device = device
        self.accData = ['', 0, 0, 0]
        self.gyroData = ['', 0, 0, 0]
        self.quatData = ['', 0, 0, 0, 0]
        self.accCallback = FnVoid_VoidP_DataP(self.acc_data_handler)
        self.gyroCallback = FnVoid_VoidP_DataP(self.gyro_data_handler)
        self.quatCallback = FnVoid_VoidP_DataP(self.quat_data_handler)

    # Quaternion callback loop to parse and gather w, x, y, z data and continuously store in .csv file
    def quat_data_handler(self, ctx, data):
        datastring = str(parse_value(data))
        wxyz_Quat_Delim = datastring.split(' ')
        wxyz_Quat_Delim[2] = wxyz_Quat_Delim[2][:-1]
        wxyz_Quat_Delim[5] = wxyz_Quat_Delim[5][:-1]
        wxyz_Quat_Delim[8] = wxyz_Quat_Delim[8][:-1]
        wxyz_Quat_Delim[11] = wxyz_Quat_Delim[11][:-1]
        wxyz_Quat = self.device.address + ',' + wxyz_Quat_Delim[2] + ',' + wxyz_Quat_Delim[5] + ',' + wxyz_Quat_Delim[8] + ',' + wxyz_Quat_Delim[11]
        self.quatData = [self.device.address, float(wxyz_Quat_Delim[2]), float(wxyz_Quat_Delim[5]), float(wxyz_Quat_Delim[8]), float(wxyz_Quat_Delim[11])]

        # with open('C:\SeniorDesign\CSVFiles\QuaternionData.csv', 'a', newline='') as csvfile:
        #     writer = csv.writer(csvfile, delimiter=' ',
        #     escapechar=' ', quoting=csv.QUOTE_NONE)
        #     writer.writerow([wxyz_Quat])

    # Accelerometer callback loop to parse and gather x, y, z data and continuously store in .csv file
    def acc_data_handler(self, ctx, data):
        datastring = str(parse_value(data))
        xyz_Acc_Delim = datastring.split(' ')
        xyz_Acc_Delim[2] = xyz_Acc_Delim[2][:-1]
        xyz_Acc_Delim[5] = xyz_Acc_Delim[5][:-1]
        xyz_Acc_Delim[8] = xyz_Acc_Delim[8][:-1]
        xyz_Acc = self.device.address + ',' + xyz_Acc_Delim[2] + ',' + xyz_Acc_Delim[5] + ',' + xyz_Acc_Delim[8]
        self.accData = [self.device.address, float(xyz_Acc_Delim[2]), float(xyz_Acc_Delim[5]), float(xyz_Acc_Delim[8])]

        # with open('C:\SeniorDesign\CSVFiles\AccelerometerData.csv', 'a', newline='') as csvfile:
        #     writer = csv.writer(csvfile, delimiter=' ',
        #     escapechar=' ', quoting=csv.QUOTE_NONE)
        #     writer.writerow([xyz_Acc])

    # Gyroscope callback loop to parse and gather x, y, z data and continuously store in .csv file
    def gyro_data_handler(self, ctx, data):
        datastring = str(parse_value(data))
        xyz_Gyro_Delim = datastring.split(' ')
        xyz_Gyro_Delim[2] = xyz_Gyro_Delim[2][:-1]
        xyz_Gyro_Delim[5] = xyz_Gyro_Delim[5][:-1]
        xyz_Gyro_Delim[8] = xyz_Gyro_Delim[8][:-1]
        xyz_Gyro = self.device.address + ',' + xyz_Gyro_Delim[2] + ',' + xyz_Gyro_Delim[5] + ',' + xyz_Gyro_Delim[8]
        self.gyroData = [self.device.address, float(xyz_Gyro_Delim[2]), float(xyz_Gyro_Delim[5]), float(xyz_Gyro_Delim[8])]

        # with open('C:\SeniorDesign\CSVFiles\GyroscopicData.csv', 'a', newline='') as csvfile:
        #     writer = csv.writer(csvfile, delimiter=' ',
        #     escapechar=' ', quoting=csv.QUOTE_NONE)
        #     writer.writerow([xyz_Gyro])

    # Function to setup and start the callback loops for IMU Sensors
    def imu_setup(self, State):
        # Configure all metawears
        print("Configuring MMRL")
        libmetawear.mbl_mw_settings_set_connection_parameters(State.device.board, 7.5, 7.5, 0, 6000)
        sleep(1.5)

        # Configure Accelerometer
        libmetawear.mbl_mw_acc_bmi160_set_odr(State.device.board, AccBmi160Odr._50Hz)
        libmetawear.mbl_mw_acc_bosch_set_range(State.device.board, AccBoschRange._4G)
        libmetawear.mbl_mw_acc_write_acceleration_config(State.device.board)

        # Configure Gyroscope                                                       # Program could not recognise a depreciated library so I had to use the hardcoded values
        libmetawear.mbl_mw_gyro_bmi160_set_range(State.device.board, 1)             # 1 = 1000Hz
        libmetawear.mbl_mw_gyro_bmi160_set_odr(State.device.board, 7)               # 7 = 50Hz
        libmetawear.mbl_mw_gyro_bmi160_write_config(State.device.board)

        # Get Accelerometer signal and subscribe
        acc = libmetawear.mbl_mw_acc_get_acceleration_data_signal(State.device.board)
        libmetawear.mbl_mw_datasignal_subscribe(acc, None, State.accCallback)

        # Get Gyroscope signal and subscribe
        gyro = libmetawear.mbl_mw_gyro_bmi160_get_rotation_data_signal(State.device.board)
        libmetawear.mbl_mw_datasignal_subscribe(gyro, None, State.gyroCallback)

        # Start Accelerometer
        libmetawear.mbl_mw_acc_enable_acceleration_sampling(State.device.board)
        libmetawear.mbl_mw_acc_start(State.device.board)

        # Start Gyroscope
        libmetawear.mbl_mw_gyro_bmi160_enable_rotation_sampling(State.device.board)
        libmetawear.mbl_mw_gyro_bmi160_start(State.device.board)

        # setup quaternion
        libmetawear.mbl_mw_sensor_fusion_set_mode(State.device.board, SensorFusionMode.NDOF)
        libmetawear.mbl_mw_sensor_fusion_set_acc_range(State.device.board, SensorFusionAccRange._8G)
        libmetawear.mbl_mw_sensor_fusion_set_gyro_range(State.device.board, SensorFusionGyroRange._2000DPS)
        libmetawear.mbl_mw_sensor_fusion_write_config(State.device.board)

        # get quat signal and subscribe
        signal = libmetawear.mbl_mw_sensor_fusion_get_data_signal(State.device.board, SensorFusionData.QUATERNION)
        libmetawear.mbl_mw_datasignal_subscribe(signal, None, State.quatCallback)

        # start acc, gyro, mag
        libmetawear.mbl_mw_sensor_fusion_enable_data(State.device.board, SensorFusionData.QUATERNION)
        libmetawear.mbl_mw_sensor_fusion_start(State.device.board)

    def imu_breakdown(self, State):
        # Breakdown/Shutdown/Cleanup
        # Stop Accelerometer
        libmetawear.mbl_mw_acc_stop(State.device.board)
        libmetawear.mbl_mw_acc_disable_acceleration_sampling(State.device.board)
        
        # Stop Gyroscope
        libmetawear.mbl_mw_gyro_bmi160_stop(State.device.board)
        libmetawear.mbl_mw_gyro_bmi160_disable_rotation_sampling(State.device.board)

        # Unsubscribe Accelerometer
        acc = libmetawear.mbl_mw_acc_get_acceleration_data_signal(State.device.board)
        libmetawear.mbl_mw_datasignal_unsubscribe(acc)
        
        # Unsubscribe Gyroscope
        gyro = libmetawear.mbl_mw_gyro_bmi160_get_rotation_data_signal(State.device.board)
        libmetawear.mbl_mw_datasignal_unsubscribe(gyro)

        # stop
        libmetawear.mbl_mw_sensor_fusion_stop(State.device.board)

        # unsubscribe to signal
        signal = libmetawear.mbl_mw_sensor_fusion_get_data_signal(State.device.board, SensorFusionData.QUATERNION)
        libmetawear.mbl_mw_datasignal_unsubscribe(signal)
        
        # Disconnect
        libmetawear.mbl_mw_debug_disconnect(State.device.board)

class EMGState:
    def __init__(self):
        self.LastPrint = time.time()
        self.FPSCounter = deque(maxlen=150)
        self.ChannelData1 = NULL
        self.ChannelData2 = NULL
        self.ChannelData3 = NULL
        self.ChannelData4 = NULL  
        print("looking for an EMG stream...")
        # Find LSL data stream coming from OpenBCI software
        self.streams = resolve_stream('type', 'EEG')
        # Create a new inlet to read from the stream
        self.inlet = StreamInlet(self.streams[0])

    def emg_pipeline(self):
        self.ChannelData1 = NULL
        self.ChannelData2 = NULL
        self.ChannelData3 = NULL
        self.ChannelData4 = NULL

        for i in range(8): # 8 channels for Cyton || 16 channels for Cyton + Daisy
            Signal, TimeStamp = self.inlet.pull_sample()

            if i == 0:
                self.ChannelData1 = Signal
            elif i == 1:
                self.ChannelData2 = Signal
            elif i == 2:
                self.ChannelData3 = Signal
            elif i == 3:
                self.ChannelData4 = Signal
'''
class ExpressionClassifier:
    def __init__(self):
        self.Expressions = ['Happy', 'Neutral', 'Angry']
        self.Reshape = (1, 520, 1)  # May need to be changed
        self.TrainingData = {}
        self.CombinedData = []
        self.TrainX = []
        self.TrainY = []
        self.TestX = []
        self.TestY = []
        self.Model = Sequential()

    def create_data_pipeline(self, DataDirectory):
        for expression in self.Expressions:
            if expression not in self.TrainingData:
                self.TrainingData[expression] = []

            ExpressionDirectory = os.path.join(DataDirectory, expression)
            for item in os.listdir(ExpressionDirectory):
                data = np.load(os.path.join(ExpressionDirectory, item))
                for item in data:
                    self.TrainingData[expression].append(item)

        length = [len(self.TrainingData[expression]) for expression in self.Expressions]
        print(length)

        for expression in self.Expressions:
            np.random.shuffle(self.TrainingData[expression])
            self.TrainingData[expression] = self.TrainingData[expression][:min(length)]

        length = [len(self.TrainingData[expression]) for expression in self.Expressions]
        print(length)

        for expression in self.Expressions:
            for data in self.TrainingData[expression]:
                if expression == 'Happy':
                    self.CombinedData.append([data, [1, 0, 0]])
                elif expression == 'Neutral':
                    self.CombinedData.append([data, [0, 1, 0]])
                elif expression == 'Angry':
                    self.CombinedData.append([data, [0, 0, 1]])

        np.random.shuffle(self.CombinedData)
        print('Length: ', len(self.CombinedData))

        for x, y in self.CombinedData:
            self.TrainX.append(x)
            self.TrainY.append(y)

        #Check shape later
        self.TrainX = np.array(self.TrainX).reshape(self.reshape)
        self.TrainY = np.array(self.TrainY)

    def create_model_cnn(self):
        self.Model.add(Conv1D(64, (3), input_shape=self.TrainX.shape[1:]))
        self.Model.add(Activation('relu'))

        self.Model.add(Conv1D(64, (2)))
        self.Model.add(Activation('relu'))
        self.Model.add(MaxPooling1D(pool_size=(2)))

        self.Model.add(Conv1D(64, (2)))
        self.Model.add(Activation('relu'))
        self.Model.add(MaxPooling1D(pool_size=(2)))

        self.Model.add(Flatten())

        self.Model.add(Dense(512))

        self.Model.add(Dense(3))
        self.Model.add(Activation('softmax'))

        self.Model.compile(loss='categorical_crossentropy',
                    optimizer='adam',
                    metrics=['accuracy'])

    def create_model_rnn(self):
        model = Sequential()

        model.add(LSTM(units=32, activation='relu', input_shape=self.TrainX.shape[:1]))

        model.add(Dense(3, activation='softmax'))

        model.compile(loss='categorical_crossentropy', 
                optimizer='adam',
                metrics=['accuracy'])

    def train_model(self):
        Epochs = 10
        BatchSize = 32

        for epoch in range(Epochs):
            self.Model.fit(self.TrainX, self.TrainY, batch_size=BatchSize, epochs=1, validation_data=(self.TestX, self.TestY))

            Score = self.Model.evaluate(self.TestX, self.TestY, batch_size=BatchSize)

            ModelName = f'NewModels/{round(Score[1]*100, 2)}-epoch-{int(time.time())}-loss-{round(Score[0], 2)}.model'
            self.Model.save(ModelName)
        
'''


# Main to setup and start the callback loops for IMU Sensors
# Functionality/Process:
# 1) Call Device = MetaWear(MAC) function and store device info in a variable: Device being the name for your device, MAC being the MAC address
# 2) Connect device by calling Device.connect()
# 3) Create and store state of device through IMUName = IMUState(Device): IMUName being some arbitrary data name, Device being the device earlier instantiated
def main():
    # Creating a UDPBroadcaster to broadcast from
    try:
        broadcast = UDPBroadcaster("localhost", int(sys.argv[1]), 1024)
    except IndexError:
        print("Too few arguments given, defaulting sending from localhost:11000")
        broadcast = UDPBroadcaster("localhost", 11000, 1024)

    # Creating variable for client port number
    try:
        port_to = int(sys.argv[2])
    except IndexError:
        print("Too few arguments given, defaulting sending to localhost:11001")
        port_to = 11001

    # Hardcoded MetaMotionRL State addresses for our devices
    MMRL_Mac1 = 'CF:79:F7:71:EC:59'
    MMRL_Mac2 = 'D2:34:22:97:6B:E9'

    # Initialize and connect devices
    State1 = MetaWear(MMRL_Mac1)
    State1.connect()
    State2 = MetaWear(MMRL_Mac2)
    State2.connect()
    
    # Instantiate devices to the IMU class
    IMU1 = IMUState(State1)
    IMU2 = IMUState(State2)

    # Start the IMU processing pipelines for the 2 IMU sensors
    IMUState.imu_setup(IMUState, State=IMU1)
    IMUState.imu_setup(IMUState, State=IMU2)

    # Press 1 after ensuring IMU devices are connected
    print('\nPress "1" after IMU Sensors are setup')
    print('Next Step: EMG Setup')

    keyboard.wait('1')

    # Instantiate EMG class and search for LSL signal
    EMG = EMGState()

    # Instantiate Classifier class
    #Classifier = ExpressionClassifier()
    SVCFile = "Assets/Scripts/Python/SVC_Linear_Mk1.sav"
    TrainedSVC = pickle.load(open(SVCFile, 'rb'))

    # Create variable to hold all IMU/EMG data to pass too classifier
    TotalChannelData = []
    ChannelData = NULL

    EMG_size = ((125) * 4)
    EMG_freq = (1/125)
    EMG_time_post = 0
    EMG_count = 0
    temparr = [0] * EMG_size
    arrayEMG = np.array(temparr)

    Gyro_size = ((3 + 3 + 4) * 2)
    Gyro_freq = (1/100)
    Gyro_time_post = 0
    Gyro_count = 0
    temparr = [0] * Gyro_size
    arrayGyro = np.array(temparr)

    resample_to = 25

    temparr = [0] * (EMG_size + Gyro_size)
    finalArray = np.array(temparr)

    # loop to process and create one massive data array for convnet classifier
    while(True):
        EMG.emg_pipeline()
        timer = time.perf_counter()
        arrayEt = []
        arrayGt = []
        ChannelData = []
        #ChannelData = [0] * (EMG_size + Gyro_size)

        if (timer - EMG_time_post >= EMG_freq) & (EMG_count < 25):
            for i in range(125):
                arrayEt.append(EMG.ChannelData1[i])
            for i in range(125):
                arrayEt.append(EMG.ChannelData2[i])
            for i in range(125):
                arrayEt.append(EMG.ChannelData3[i])
            for i in range(125):
                arrayEt.append(EMG.ChannelData4[i])
            if arrayEMG == []:
                arrayEMG = arrayEt
            else:
                arrayEMG = np.vstack([arrayEMG, arrayEt])
            EMG_time_post = time.perf_counter()
            EMG_count += 1

        if (timer - Gyro_time_post >= Gyro_freq) & (Gyro_count < 20):
            for i in range(3):
                arrayGt.append(IMU1.accData[i+1])
            for i in range(3):
                arrayGt.append(IMU1.gyroData[i+1])
            for i in range(4):
                arrayGt.append(IMU1.quatData[i+1])
            for i in range(3):
                arrayGt.append(IMU2.accData[i+1])
            for i in range(3):
                arrayGt.append(IMU2.gyroData[i+1])
            for i in range(4):
                arrayGt.append(IMU2.quatData[i+1])
            if arrayGyro == []:
                arrayGyro = arrayGt
            else:
                arrayGyro = np.vstack([arrayGyro, arrayGt])
            Gyro_time_post = time.perf_counter()
            Gyro_count += 1

        if (EMG_count == 25) & (Gyro_count == 20):
            arrayRt = []
            resample_array = signal.resample(arrayGyro, resample_to, axis = 0)
            i = 0
            while(i < resample_to):
                arrayRt = np.append(arrayEMG[i, :], resample_array[i, :], axis = 0)
                if i == 0:
                    ChannelData = arrayRt
                else:
                    ChannelData = np.vstack([ChannelData, arrayRt])
                i += 1
            EMG_count = 0
            Gyro_count = 0
            arrayEMG = []
            arrayGyro = []
            TotalChannelData.append(ChannelData)

            Prediction = TrainedSVC.predict(ChannelData)
            HappyPrediction = 0
            NeutralPrediction = 0
            AngryPrediction = 0
            for i in range(25):
                if(Prediction[i] == 0):
                    NeutralPrediction += 1
                elif(Prediction[i] == 1):
                    AngryPrediction += 1
                elif(Prediction[i] == 2):
                    HappyPrediction += 1
            HappyPrediction = HappyPrediction / 25
            NeutralPrediction = NeutralPrediction / 25
            AngryPrediction = AngryPrediction / 25

            # Creates three cosine waves with 120 degree seperation and a domain of [0,1]
            out = ThreeTuple(NeutralPrediction, HappyPrediction, AngryPrediction)
            # End changes here
            # Encodes bytes as a set of three floats, and sends it to client process
            out_bytes = out.encode_as_floats()
            broadcast.broadcast("localhost", port_to, out_bytes)
            # print('Neutral: ' + str(NeutralPrediction) + ', Angry: ' + str(AngryPrediction) + ', Happy: ', str(HappyPrediction))
            
        # print('IMU/EMG Data Array Size: ', len(ChannelData))

        # Uncomment if you want to save EMG Data to .csv
        # with open('C:\SeniorDesign\CSVFiles\EMGData.csv', 'a', newline='') as csvfile:
        #     writer = csv.writer(csvfile, delimiter=' ',
        #     escapechar=' ', quoting=csv.QUOTE_NONE)
        #     writer.writerow([EMG.ChannelData1])
        
        # Uncomment to add fake delay into system to simulate Hz range
        # sleep(1/20)

        # Press 2 to stop processing / classifier loop
        if(keyboard.is_pressed('2')):
            break


    # Comment out when you do not need new training data
    # --- Code block to create new training data or testing data ---
    # Expression used for training data
    # Expression = 'Happy'
    # Expression = 'Neutral'
    # Expression = 'Angry'

    # Data storage locations
    # DataDirectory = 'C:\\Users\\codyt\\Desktop\\UTASchool\\SeniorDesign\\ConvNetData'
    # ValidationDataDirectory = 'C:\\Users\\codyt\\Desktop\\UTASchool\\SeniorDesign\\ConvNetValidationData'
    # ExpressionDirectory = f'C:\\Users\\codyt\\Desktop\\UTASchool\\SeniorDesign\\ConvNetData\\{Expression}'
    # ValidationExpressionDirectory = f'C:\\Users\\codyt\\Desktop\\UTASchool\\SeniorDesign\\ConvNetValidationData\\{Expression}'

    # print(f'Saving {Expression} data...')
    # np.save(os.path.join(ExpressionDirectory, 'Neutral_5.npy'), np.array(TotalChannelData)) # f'{int(time.time())}
    # --------------------------------------------------------------

 
    # Comment out if you do not need to train the ConvNet
    # --- Creates data to be passed to ConvNet ---
    # Classifier.create_data_pipeline(ExpressionClassifier, DataDirectory)
    # Classifier.create_model_rnn(ExpressionClassifier)
    # Classifier.train_model(ExpressionClassifier)
    # --------------------------------------------
    
    # Breakdown/Cleanup of IMU sensors
    IMUState.imu_breakdown(IMUState, State=IMU1)
    IMUState.imu_breakdown(IMUState, State=IMU2)

if __name__ == "__main__":
    main()

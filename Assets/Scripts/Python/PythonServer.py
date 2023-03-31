# Command Line Args: port_from, port_to
import sys
import time
import struct
import math
import logging
from UDPBroadcaster import UDPBroadcaster

# Setup for logging file
logging.basicConfig(filename='Logs/pythonserver.log', encoding='utf-8', level=logging.DEBUG, filemode="w")


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


# Creating a UDPBroadcaster to broadcast from
try:
    broadcast = UDPBroadcaster("localhost", int(sys.argv[1]), 1024)
except IndexError:
    print("Too few arguments given, defaulting sending from localhost:11000")
    broadcast = UDPBroadcaster("localhost", 11000, 1024)
except Exception as e:
    logging.exception(e)
    raise e

# Creating variable for client port number
try:
    port_to = int(sys.argv[2])
except IndexError:
    print("Too few arguments given, defaulting sending to localhost:11001")
    port_to = 11001
except Exception as e:
    logging.exception(e)
    raise e

try:
    # Generating/Sending data for testing
    start_time = time.time()
    while(True):
        # Change this to take in sensor data and classify it instead
        current_time = time.time() - start_time

        # Creates three cosine waves with 120 degree seperation and a domain of [0,1]
        out = ThreeTuple((1 + math.cos(current_time)) / 2, (1 + math.cos(current_time + 2 * math.pi * 0.333)) / 2,
                         (1 + math.cos(current_time + 2 * math.pi * 0.666)) / 2)
        # End changes here
        # Encodes bytes as a set of three floats, and sends it to client process
        out_bytes = out.encode_as_floats()
        broadcast.broadcast("localhost", port_to, out_bytes)
        time.sleep(0.01)
except Exception as e:
    logging.exception(e)
    raise e
finally:
    logging.info("Closed without exception.")
    broadcast.Close()
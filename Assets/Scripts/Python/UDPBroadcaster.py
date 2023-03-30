import socket

class UDPBroadcaster:
    # Opens a socket to broadcast from and binds to it
    def __init__(self, server_ip, server_port, buffer_size):
        self.server_ip = server_ip
        self.server_port = server_port
        self.buffer_size = buffer_size
        self.UDP_socket = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
        self.UDP_socket.bind((server_ip, server_port))

    # Takes in a series of bytes, concatenates them, and sends them to an address
    def broadcast(self, ip_to, port_to, *data):
        bytes_to_send = bytearray()
        for d in data:
            bytes_to_send += d
        try:
            print(f'Sending: {bytes_to_send}')
            self.UDP_socket.sendto(bytes_to_send, (ip_to, port_to))
        except:
            raise

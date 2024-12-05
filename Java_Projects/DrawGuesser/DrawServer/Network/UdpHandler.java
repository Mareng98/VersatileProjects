package Network;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.nio.ByteBuffer;
import java.util.List;

import Server.ClientManager;
import Server.DrawServer;
import Server.Client;

/**
 * Handles all UDP traffic between clients.
 */
public class UdpHandler implements Runnable {
    private final int port; // The port to listen at

    /**
     * Initialize the handler
     * @param port the port to listen at for UDP traffic.
     */
    public UdpHandler(int port) {
        this.port = port;
    }

    /**
     * Continuously listens for new drawing data received from the current painter to broadcast to
     * all other clients.
     */
    @Override
    public void run() {
        try (DatagramSocket udpSocket = new DatagramSocket(port)) {
            System.out.println("Server is listening for UDP messages on port " + port);
            byte[] responseData = new byte[12]; // id, x-coordinate, y-coordinate
            byte[] requestData = new byte[8]; // x-coordinate, y-coordinate
            while (true) {
                DatagramPacket receivedMessage = new DatagramPacket(responseData, responseData.length);
                // Listen for a new message
                udpSocket.receive(receivedMessage);
                // Extract the ID of the client
                ByteBuffer byteBuffer = ByteBuffer.wrap(responseData);
                int id = byteBuffer.getInt();
                // Ensure that the received message is from the current painter, otherwise ignore it
                if(DrawServer.isCurrentPainter(id)){
                    // Find all other clients to broadcast the painter's message to
                    List<Client> clients = ClientManager.getClientsExcluding(id);
                    // Copy contents of bytebuffer into requestData (without the id part)
                    byteBuffer.position(4); // Ensure that the bytebuffer is pointing to the second integer
                    byteBuffer.get(requestData); // Copy the next 8 bytes to requestData
                    // Relay the message to all other clients
                    for(Client client : clients){
                        DatagramPacket request = new DatagramPacket(requestData, requestData.length,
                                client.getAddress(), client.getUDP_PORT());
                        udpSocket.send(request);
                    }
                }
            }
        } catch (IOException e) {
            System.out.println("Something went wrong when listening for UDP traffic: " + e.getMessage());
        }
    }
}
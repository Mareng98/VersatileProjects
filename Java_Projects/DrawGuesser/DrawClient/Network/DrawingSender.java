package Network;

import Client.DrawClient;

import java.awt.*;
import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.nio.ByteBuffer;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

/**
 * Constantly sends newly drawn points to the server.
 */
public class DrawingSender extends Thread{
    private static BlockingQueue<Point> pointsToSend; // The points to be sent
    private final int UDP_PORT; // udp port of server
    private final String SERVER_ADDRESS; // Address of server
    private final DatagramSocket socket; // The socket to be used for sending messages
    private volatile boolean alive = true;

    /**
     * Kill this thread.
     */
    public void kill(){
        alive = false;
    }

    /**
     * Initialize the drawing sender.
     * @param UDP_PORT udp port of server
     * @param SERVER_ADDRESS Address of server
     * @param socket The socket to be used for sending messages
     */
    public DrawingSender(int UDP_PORT, String SERVER_ADDRESS, DatagramSocket socket) {
        // Initialize variables
        pointsToSend = new LinkedBlockingQueue<>();
        this.UDP_PORT = UDP_PORT;
        this.SERVER_ADDRESS = SERVER_ADDRESS;
        this.socket = socket;
        this.start();
    }

    /**
     * Continuously send points drawn on the canvas to the server while alive.
     */
    @Override
    public void run() {
        // Continuously listen for new points to send to other clients
        try {
            // Get host address
            InetAddress host = InetAddress.getByName(SERVER_ADDRESS);
            // Create UDP request packet
            int dataLength = 12;
            byte[] requestData = new byte[dataLength];
            DatagramPacket request = new DatagramPacket(requestData, requestData.length, host, UDP_PORT);
            // UDP hole punching (Required if we're running a public server behind a home router)
            punchUdpHole(socket, host);
            while (alive) {
                // Waits until there's a new point to send
                Point p = pointsToSend.poll(1, TimeUnit.SECONDS);
                // Ensure there is a point to send
                if(p != null){
                    // Use ByteBuffer to copy the point's integers into the byte array
                    ByteBuffer buffer = ByteBuffer.allocate(dataLength);
                    // Put ID, x, and y into 4 bytes of the buffer each
                    buffer.putInt(DrawClient.getId());
                    buffer.putInt(p.x);
                    buffer.putInt(p.y);
                    // Copy the ByteBuffer's content to requestData
                    System.arraycopy(buffer.array(), 0, requestData, 0, dataLength);
                    // Transmit the point
                    socket.send(request);
                }

            }
        } catch (InterruptedException e) {
            System.err.println("Drawing sender thread was interrupted");
        } catch (UnknownHostException e) {
            System.err.printf("Could not connect to %s\n%s", SERVER_ADDRESS, e.getMessage());
        } catch (IOException e) {
            System.err.printf("Could not send point to %s\n%s", SERVER_ADDRESS, e.getMessage());
        }
    }

    /**
     * To enable receiving UDP traffic from the server, we may need to perform "UDP hole punching".
     * This sends a minimal packet to the server, allowing the router/firewall to open a path for incoming traffic.
     * @param socket the socket used for sending the message
     * @param host the address of the server
     * @throws IOException if the message fails to send
     */
    private void punchUdpHole(DatagramSocket socket, InetAddress host) throws IOException {
        byte[] message = new byte[1]; // Send some minimal data
        DatagramPacket packet = new DatagramPacket(message, message.length, host, UDP_PORT);
        socket.send(packet);
    }

    /**
     * Adds a drawn point to the outgoing message buffer to be sent.
     * @param p The point to add.
     */
    public static void addPointToSend(Point p){
        pointsToSend.add(p);
    }
}

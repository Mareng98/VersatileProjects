package Network;

import Client.DrawClient;

import java.awt.*;
import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.SocketTimeoutException;
import java.nio.ByteBuffer;

/**
 * This class handles the reading of incoming points sent by the other client.
 */
public class DrawingListener extends Thread {
    final DatagramSocket socket; // The socket to be read from
    private volatile boolean alive = true;

    /**
     * Kill this thread.
     */
    public void kill(){
        alive = false;
    }

    /**
     * Initiate the DrawingListener.
     */
    public DrawingListener(DatagramSocket socket) {
        this.socket = socket;
        this.start();
    }

    /**
     * Continuously listen for new messages containing new points to
     * draw from the socket while the thread is alive.
     */
    @Override
    public void run() {
        byte[] responseData = new byte[8];
        DatagramPacket response = new DatagramPacket(responseData, responseData.length);
        while (alive) {
            // Attempt to read the next message for 1 second
            try {
                socket.setSoTimeout(1000);
                try{
                    socket.receive(response);
                    // Convert byte data to a point
                    ByteBuffer byteBuffer = ByteBuffer.wrap(responseData);
                    Point p = new Point(byteBuffer.getInt(), byteBuffer.getInt());
                    // Add to draw
                    DrawClient.addPointToDraw(p);
                }catch (SocketTimeoutException e){
                    // Ignore timeout exception
                }
            } catch (IOException e) {
                System.out.println(e.getMessage());
            }
        }
    }
}

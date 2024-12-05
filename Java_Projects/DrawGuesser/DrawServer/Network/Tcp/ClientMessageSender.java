package Network.Tcp;

import Server.Client;

import java.io.PrintWriter;

/**
 * Constantly sends messages to the client that are added to their outgoing tcp message buffer.
 */
public class ClientMessageSender extends Thread {

    private final PrintWriter out;
    private final Client client;
    private boolean alive = true;
    public void kill(){
        alive = false;
    }

    /**
     * Initializes the message sender
     * @param out The client-socket's output stream.
     * @param client The client to send messages to.
     */
    public ClientMessageSender(PrintWriter out, Client client){
        this.out = out;
        this.client = client;
    }

    /**
     * Polls outgoing message buffer of a client and sends them while the thread is alive.
     */
    @Override
    public void run() {
        while (alive) {
            // Continuously block thread for 1 second until a message is available in the queue
            String message = client.pollTcpMessageToSend();
            if (message != null) {
                out.println(message);  // Send the message
            }
        }
    }
}

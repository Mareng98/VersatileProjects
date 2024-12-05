package Network;

import java.io.PrintWriter;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingDeque;
import java.util.concurrent.TimeUnit;

/**
 * Constantly looks in the chat buffer for new messages to send to the server.
 */
public class MessageWriter extends Thread {
    private final PrintWriter out;
    // The chat buffer containing outgoing messages
    private final static BlockingQueue<String> blockingChatQueue = new LinkedBlockingDeque<>();
    private volatile boolean alive = true; // Flag for thread to know when to return

    /**
     * Initialize the Message Writer with a print writer
     * @param out the print writer to use.
     */
    public MessageWriter(PrintWriter out) {
        this.out = out;
    }

    /**
     * Kill the message writer.
     */
    public void kill(){
        alive = false;
    }

    /**
     * Constantly looks in the chat buffer for new messages to send to the server.
     */
    @Override
    public void run() {
        String message;

        try {
            // Keep sending new messages while alive
            while (alive) {
                // Wait for a new message to send for 1 second
                message = blockingChatQueue.poll(1,TimeUnit.SECONDS);
                if(message != null){
                    out.println(message);
                }
            }
        } catch (InterruptedException e) {
            System.out.println("MessageWriter interrupted!\n" + e);
        }

    }

    /**
     * Add a regular message to the outgoing message buffer.
     * @param message the message to add.
     */
    public static void addTcpMessage(String message) {
        if(message.startsWith("/g ")){
            blockingChatQueue.add(message); // Add guess
        }else if(!message.startsWith("/s ")){
            blockingChatQueue.add("/m " + message); // Add regular message
        }
    }

    /**
     * Add a system message to the outgoing message buffer.
     * @param message the message to add.
     */
    public static void addTcpSystemMessage(String message) {
        blockingChatQueue.add("/s " + message);
    }
}

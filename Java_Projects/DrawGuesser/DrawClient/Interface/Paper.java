package Interface;

import Network.DrawingSender;
import Client.DrawClient;

import javax.swing.*;
import java.awt.*;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.awt.event.MouseMotionAdapter;
import java.util.HashSet;
import java.util.Iterator;
import java.util.concurrent.Semaphore;

/**
 * The canvas to be drawn to.
 */
public class Paper extends JPanel {
    // List to hold all points to be drawn
    private HashSet hs = new HashSet();
    // Semaphore to provide mutual exclusion to the hash set so that we can multi-thread
    private final Semaphore semaphore = new Semaphore(1);

    /**
     * Instantiate event listeners and background
     */
    public Paper() {
        setBackground(Color.white);
        addMouseListener(new L1());
        addMouseMotionListener(new L2());
    }

    /**
     * Draw all points
     *
     * @param g the <code>Graphics</code> object to protect
     */
    public void paintComponent(Graphics g) {
        super.paintComponent(g);
        g.setColor(Color.black);
        try {
            // Acquire the semaphore before modifying the set
            semaphore.acquire();
            // Draw all points
            Iterator i = hs.iterator();
            while (i.hasNext()) {
                Point p = (Point) i.next();
                g.fillOval(p.x, p.y, 2, 2);
            }
        } catch (InterruptedException e) {
            System.out.println(e.getMessage());
        } finally {
            // Release the semaphore after modification
            semaphore.release();
        }
    }

    /**
     * Adds a point that has been received by other clients to be drawn
     *
     * @param p The point to be drawn
     */
    public void addReceivedPoint(Point p) {
        try {
            // Acquire the semaphore before modifying the set
            semaphore.acquire();
            // Add point
            hs.add(p);
            repaint();
        } catch (InterruptedException e) {
            System.out.println(e.getMessage());
        } finally {
            // Release the semaphore after modification
            semaphore.release();
        }
    }

    /**
     * Adds a point to be drawn and requests for point to be sent to other client
     *
     * @param p The point to be drawn
     */
    private void addPoint(Point p) {
        try {
            // Acquire the semaphore before modifying the set
            semaphore.acquire();
            // Add and send point
            hs.add(p);
            DrawingSender.addPointToSend(p);
            repaint();
        } catch (InterruptedException e) {
            System.out.println(e.getMessage());
        } finally {
            // Release the semaphore after modification
            semaphore.release();
        }
    }

    /**
     * Handles drawing of point when mouse is clicked
     */
    class L1 extends MouseAdapter {
        public void mousePressed(MouseEvent me) {
            // Ensure the client is allowed to paint
            if(DrawClient.isClientCurrentPainter()){
                addPoint(me.getPoint());
            }

        }
    }

    /**
     * Handles drawing of a line when mouse is dragged
     */
    class L2 extends MouseMotionAdapter {
        public void mouseDragged(MouseEvent me) {
            // Ensure the client is allowed to paint
            if(DrawClient.isClientCurrentPainter()){
                addPoint(me.getPoint());
            }
        }
    }

    /**
     * Clears all drawn points from the paper.
     */
    public void clear() {
        try {
            // Acquire the semaphore before modifying the set
            semaphore.acquire();
            // Clear all points
            hs.clear();
            // Repaint the panel to reflect the cleared state
            repaint();
        } catch (InterruptedException e) {
            System.out.println(e.getMessage());
        } finally {
            // Release the semaphore after modification
            semaphore.release();
        }
    }
}

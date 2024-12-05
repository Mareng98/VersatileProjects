package Interface;

import javax.swing.*;
import java.awt.*;

/**
 * A circular timer that counts down from a specified number of seconds that's used in the GUI.
 */
public class CircularTimer extends JPanel {
    private final int totalSteps; // Total number of steps on the clock
    private int currentStep;
    private boolean countingDown = false;

    /**
     * Initialize the timer with the maximum number of seconds it can count down from.
     * @param totalSteps Maximum amount of time.
     */
    public CircularTimer(int totalSteps) {
        this.totalSteps = totalSteps;
        this.currentStep = 0; // Starts the counter at step 0 (Empty circle)
    }

    /**
     * Start the countdown from the specified startStep
     * @param startStep The number of seconds to count down from.
     */
    public void startCountdown(int startStep) {
        // Initialize countdown
        countingDown = true;
        currentStep = startStep;

        // Start counting down once a second
        Timer timer = new Timer(1000, e -> {
            // Make sure we should continue counting down
            if (currentStep > 0 && countingDown) {
                // decrement current step and update
                currentStep--;
                repaint();
            } else {
                // Reset countdown
                countingDown = false;
                currentStep = 0;
                repaint();
                // Get timer from event and stop it
                ((Timer) e.getSource()).stop();
            }
        });

        // Start the countdown
        timer.start();
    }

    /**
     * Get the display color for the arc based on the current step.
     * Used to create a sense of urgency and to make it prettier.
     * @return The color to display the arc as.
     */
    private Color getCurrentColor() {
        if (currentStep > 45) return Color.GREEN;
        if (currentStep > 25) return Color.YELLOW;
        if (currentStep > 10) return Color.ORANGE;
        return Color.RED;
    }

    /**
     * Draw the timer with a circle containing text and an arc showing the time.
     * @param g the <code>Graphics</code> object to protect
     */
    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g);
        int diameter = Math.min(getWidth(), getHeight()) - 20; // Set diameter with padding
        int x = (getWidth() - diameter) / 2; // x position on GUI
        int y = (getHeight() - diameter) / 2; // y position on GUI

        // Draw background circle slightly larger than the diameter of the progress-circle
        g.setColor(new Color(86, 50, 168));
        g.fillOval(x-2, y-2, diameter+4, diameter+4);

        // Draw progress arc based on current step
        g.setColor(getCurrentColor());
        int arcAngle = (int) ((double) currentStep / totalSteps * 360);

        // Arc starts from the bottom and decreases clock-wise
        g.fillArc(x, y, diameter, diameter, -90, arcAngle);

        // Draw circle roughly in the middle where the text is
        g.setColor(new Color(86, 50, 168));
        g.fillOval(x + diameter/4 - 1, y + diameter/4 - 1, diameter/2 + 2, diameter/2 + 2);

        // Draw current time in the center of circle
        g.setColor(Color.WHITE);
        g.setFont(new Font("Monospaced", Font.BOLD, 13));
        String stepText = String.valueOf(currentStep);

        // Since the time changes we need to get the dynamic width of the text to center it
        FontMetrics fm = g.getFontMetrics();
        int textWidth = fm.stringWidth(stepText);
        int textHeight = fm.getAscent();

        // Draw the step number roughly centered in the component
        g.drawString(stepText, (getWidth() - textWidth) / 2, (getHeight() + textHeight) / 2 - 3);
    }

    /**
     * Stop counting down and reset timer.
     */
    public void resetCounter() {
        countingDown = false;
    }
}

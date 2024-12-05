package Utility;

import java.util.Random;

/**
 * A helper class to generate a "random" word.
 */
public class WordGenerator {
    // Words that can be guessed
    private static final String[] words = {
            "ferris wheel",
            "barricade",
            "belt",
            "knot",
            "pet",
            "wing",
            "stove",
            "suitcase",
            "dump truck",
            "step",
            "angel",
            "drawer",
            "plate",
            "black hole",
            "trapeze",
            "oven",
            "trumpet",
            "fairies",
            "chimney",
            "marry",
            "lemon",
            "run",
            "dustpan",
            "tie",
            "campfire",
            "sunburn",
            "loaf",
            "panda",
            "liquid",
            "cactus",
            "stain",
            "sailboat",
            "bat",
            "key",
            "fanny pack",
            "zebra",
            "time",
            "light switch",
            "pea",
            "wave",
            "lighthouse",
            "ticket",
            "spare",
            "save",
            "eclipse",
            "pen",
            "hammer",
            "marker"
    };

    /**
     * Get a random word from the word list
     * @return The next word
     */
    public static String getNextWord() {
        Random random = new Random();
        return words[random.nextInt(words.length)];
    }
}

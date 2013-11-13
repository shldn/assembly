
// This is the Environment.
public class Environment {

    private static int calories = 100;

    public static bool IsFit(Assembly assembly)
    {
        return assembly.nodes.Count > 20;
    }

    // Accessors
    public static int CaloriesAvailable { get { return calories; } set { calories = value; } }
}

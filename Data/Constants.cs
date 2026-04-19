namespace MauiApp1.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "AppSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        // Supabase Configuration - Replace with your actual values
        public const string SupabaseUrl = "https://your-project.supabase.co";
        public const string SupabaseKey = "your-anon-key";
    }
}
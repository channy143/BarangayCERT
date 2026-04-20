namespace MauiApp1.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "AppSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        // Supabase Configuration
        public const string SupabaseUrl = "https://usqyftqajeypdukdvces.supabase.co";
        public const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVzcXlmdHFhamV5cGR1a2R2Y2VzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzYxMjcwNTMsImV4cCI6MjA5MTcwMzA1M30.fN5XBr8s4_LEPLjYVaWAhJv14tzT0yZC4OK1YJzcVw4";
    }
}
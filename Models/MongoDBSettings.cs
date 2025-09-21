namespace Backend.Models
{
    public class MongoDBSettings
    {
        public required string ConnectionString { get; set; }
        public required string DatabaseName { get; set; }

        public string AdminDatabaseName { get; set; }
        public string UsersCollectionName { get; set; }

        public string UserDetailsCollectionName { get; set; }

        public string UserDetailsdatabase { get; set; }

        public string PasswordResetCollectionName { get; set; }

        public string HoneyCombCollectionName { get; set; }

        public string HoneyCombDatabaseName { get; set; }
        
    }
}

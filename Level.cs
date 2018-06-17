namespace PR2_Client
{
    class Level
    {
        public Level()
        {
            slots = new string[4];
        }
        public string title;
        public int level_id;
        public int version;
        public string[] slots;
    }
}
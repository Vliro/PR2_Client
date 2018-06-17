using System;

namespace PR2_Client
{
    class LevelHandler
    {
        private Level[] m_Levels;
        public LevelHandler(Level[] levels)
        {
            m_Levels = levels;
        }

        public void updateLevelSlot(short slot, string name, int level, int level_id)
        {
            Level m_Level = Array.Find(m_Levels, m => m.level_id == level_id);
            if(m_Level != null && level != -1)
            {
                m_Level.slots[slot] = level + name;
            } else {
                m_Level.slots[slot] = "";
            }
            WriteLevels(true);
        }
        public void WriteLevels(bool clearTerm)
        {
            if(clearTerm)
                Console.Clear();
            int size = (int)Math.Sqrt(m_Levels.Length);
            for(int l = 0; l < size; l++)
            {
                int i = l*size;
                Console.Write(m_Levels[i].title + "\t" + m_Levels[i+1].title + "\t" + m_Levels[i+2].title + "\n");
                for(int j = 0; j < 4; j++)
                {
                    for(int k = 0; k < size; k++)
                    {
                        if(m_Levels[i+k].slots[j] != "")
                        {
                            Console.Write("[");
                            Console.Write(m_Levels[k + i].slots[j]);
                            Console.Write("]" + "\t");
                        } else {
                            Console.Write("[]" + "\t");
                        }
                    }
                    Console.Write("\n");
                }
                Console.Write("\n");
            }
        }
    }
}
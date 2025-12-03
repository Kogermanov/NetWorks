using System.Text;

namespace SendingFiles
{
    class FileCreator
    {
        string name;
        string directory;
        public FileCreator (string directory, string name)
        {
            this.name = name;
            this.directory = directory;
        }

        public string CreateName ()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(directory).Append(name);
            RenameFile(builder);
            return builder.ToString();
        }

        void RenameFile (StringBuilder builder)
        {
            int ind = 0;
            for (int i = builder.Length - 1; i >= 0; i--)
            {
                if (builder[i] == '.')
                {
                    ind = i;
                    break;
                }
            }

            bool isFirst = false;
            int count = 0;
            while (File.Exists(builder.ToString())) 
            {
                if (!isFirst)
                {
                    builder.Insert(ind, $"{count}");
                    isFirst = true;
                }
                else
                {   
                    builder.Remove(ind, count.ToString().Length);
                    count++;
                    builder.Insert(ind, $"{count}");
                }
                
            }
        }
    }
}
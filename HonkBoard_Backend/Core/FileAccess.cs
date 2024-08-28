//using Newtonsoft.Json;
//using System;
//using System.IO;
//using System.Reflection;
//using HonkBoard_Backend.Core.Structures;
//using HonkBoard_Backend.Core.Strutures;


//namespace HonkBoard_Backend.Core
//{
//    public class FileAccess : IDataAccess
//    {
//        private readonly string _folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Data\\";
//        private readonly string _filePath;

//        public FileAccess()
//        {
//            _filePath = _folderPath + "userData";

//            if (!File.Exists(_filePath))
//            {

//                if(!Directory.Exists(_folderPath))
//                {

//                    Directory.CreateDirectory(_folderPath);

//                }

//                File.Create(_filePath);
//            }
//        }

//        public int GetFreeId()
//        {
//            //var id = 1;
//            //var file = File.OpenText(_filePath);
//            //var serializer = new JsonSerializer();

//            //while (!file.EndOfStream)
//            //{

//            //    var obj = (User)serializer.Deserialize(file, typeof(User));

//            //    if (obj.Id == id)
//            //    {

//            //        id ++;

//            //    }
//            //}
//            //file.Close();

//            return 1;

//        }


//        public Participant GetInfo(int id)
//        {

//            //var serializer = new JsonSerializer();
//            //var file = File.OpenText(_filePath);
//            //while (!file.EndOfStream)
//            //{

//            //    var obj = (User)serializer.Deserialize(file, typeof(User));

//            //    if (obj.Id == id)
//            //    {

//            //        file.Close();
//            //        return obj;
                   
//            //    }
//            //}
//            //file.Close();
//            return null; // Объект с указанным идентификатором не найден

//        }


//        public void WriteInfo(User userInformation)
//        {

//            var json = JsonConvert.SerializeObject(userInformation);
//            File.AppendAllText(_filePath , json);
          
//        }
       
//        public async Task<Participant> FindByGoogleId(string googleId)
//        {

//            var file = File.OpenText(_filePath);
//            var serializer = new JsonSerializer();

//            while (!file.EndOfStream)
//            {

//                var obj = (Participant)serializer.Deserialize(file, typeof(Participant));

//                if (obj.Id == googleId)
//                {

//                    file.Close();
//                    return obj;

//                }
//            } 
//            file.Close();
           
//            return null;

//        }

//        public Task<bool> IsRegistered(string googleId)
//        {
//            throw new NotImplementedException();
//        }

//        Task IDataAccess.WriteInfo(User userInformation)
//        {
//            throw new NotImplementedException();
//        }

//        public Task PatchUser(User userInformation)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<User> GetUser(string googleId)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
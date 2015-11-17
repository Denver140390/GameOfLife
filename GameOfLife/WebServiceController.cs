using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace GameOfLife
{
    class WebServiceController
    {
        public byte[,] Request(string postData)
        {


            string url = "http://game-of-life-ws.herokuapp.com/next";
            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";

            ///TODO input parameter
//            string postData = "[[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]," +
//                              "[1,0,0,1,0,1,1,0,1,1]]";

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/json";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;
            // Get the request stream.
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Close the Stream object.
            dataStream.Close();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            byte[,] nextgen = JsonConvert.DeserializeObject<byte[,]>(responseFromServer);

            // Clean up the streams.
            reader.Close();
            response.Close();

            return nextgen;
        }
    }
}

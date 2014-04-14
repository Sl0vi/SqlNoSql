// The MIT License (MIT)
//
// Copyright (c) 2014 Bernhard Johannessen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace SqlNoSql.Data
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Bson;
    using System.IO;

    public static class Serializer
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        };

        public static string SerializeJson<T>(T document)
        {
            return JsonConvert.SerializeObject(document, Formatting.None, settings);
        }

        public static T DeserializeJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static byte[] SerializeBson<T>(T document)
        {
            var memoryStream = new MemoryStream();
            var bsonWriter = new BsonWriter(memoryStream);
            var serializer = new JsonSerializer();
            serializer.Serialize(bsonWriter, document);
            return memoryStream.ToArray();
        }

        public static T DeserializeBson<T>(byte[] bson)
        {
            var memoryStream = new MemoryStream(bson);
            var bsonReader = new BsonReader(memoryStream);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<T>(bsonReader);
        }
    }
}

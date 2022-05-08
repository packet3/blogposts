# blogposts
all code relating to blog posts
public class CreateEventCommand : IRequest<string>
    {
        public string ChangeType { get; set; }
        public IDictionary<string, string> MessageBody { get; set; }
    }
  
  
       Person person = new Person();

                var testing = request.MessageBody;

                var json = JsonConvert.SerializeObject(testing, Newtonsoft.Json.Formatting.Indented);
                var myobject = JsonConvert.DeserializeObject<Person>(json);
  

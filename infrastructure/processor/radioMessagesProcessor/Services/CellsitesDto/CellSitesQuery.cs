namespace radioMessagesProcessor.Services
{
    public class CellSitesQuery
    {
        public Responseheader responseHeader { get; set; }
        public Response response { get; set; }
    }

    public class Responseheader
    {
        public int status { get; set; }
        public int QTime { get; set; }
        public Params _params { get; set; }
    }

    public class Params
    {
        public string q { get; set; }
        public string pt { get; set; }
        public string d { get; set; }
        public string fq { get; set; }
    }

    public class Response
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public CellSiteSolr[] docs { get; set; }
    }

    public class CellSiteSolr
    {
        public string radio { get; set; }
        public string mcc { get; set; }
        public string net { get; set; }
        public string area { get; set; }
        public string cell { get; set; }
        public string unit { get; set; }
        public string lon { get; set; }
        public string lat { get; set; }
        public int range { get; set; }
        public int samples { get; set; }
        public int changeable { get; set; }
        public long[] created { get; set; }
        public long updated { get; set; }
        public string id { get; set; }
        public string location { get; set; }
        public long _version_ { get; set; }
    }
}
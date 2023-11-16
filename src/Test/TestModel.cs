namespace MyApp.Test {
    public class TestModel {
        public int id { get; set; } = 0;
        public int number_normal { get; set; }
        public int? number_nullable { get; set; } = null;
        public string normal_str { get; set; } = "";
        public string? str_nullable { get; set; }
        public DateTime my_date { get; set; }
        public DateTime? date_nullable { get; set; }
        public Other other_class { get; set; } = new Other();

        public TestModel() { }
    }
}
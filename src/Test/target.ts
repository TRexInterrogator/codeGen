interface ITestModel {
    id: number;
    number_normal: number;
    number_nullable: number | null;
    normal_str: string;
    str_nullable: string | null;
    my_date: string;
    date_nullable: string | null;
}

export class TestModel implements ITestModel {

    public id = 0;
    public number_normal = 0;
    public number_nullable: number | null = null;
    public normal_str = "";
    public str_nullable: string | null = null;
    public my_date = new Date().toISOString();
    public date_nullable: string | null = null;

    public static CreateInstance(source: ITestModel): TestModel {
        const instance = new TestModel();
        Object.assign(source, instance);
        return instance;
    }
}
public class Respository
{
    private Data _data;
    public Respository(Data data)
    {
        _data = data;
    }
    public int RowCount => _data.RowCount;
}
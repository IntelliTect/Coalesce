namespace Coalesce.Starter.Vue.Data.Coalesce;

public abstract class AppDataSource<T> : StandardDataSource<T, AppDbContext>
    where T : class
{
    protected AppDataSource(CrudContext<AppDbContext> context) : base(context)
    {
    }
}

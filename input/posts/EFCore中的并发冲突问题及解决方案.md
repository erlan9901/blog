Title : EF Core中的并发冲突问题及解决方案

Date : 2023-5-23

Tag : C# .NET

---

## EF Core中的并发冲突问题及解决方案

在开发系统过程中，我们有时会遇到这样一个问题：

用户A读取一条数据，然后准备更新数据，同时用户B也读取了这条数据，但此时用户A还没有把这条数据更新完成，所以B读取的还是之前的数据。B看到后，心想，这条数据仍没变化，就也想更新它。此时正好用户A和B同时去更新这条数据，导致A输入的数据被B覆盖，或者B被A覆盖，看谁先提交，谁就被覆盖掉。我们肯定不希望这样的事发生，于是就有了并发冲突控制。

解决并发冲突问题，一般是给数据加锁，锁分为两种，一种是悲观锁，一种是乐观锁。EFCore本身不支持悲观锁，如果非要使用，只能通过事务 + 执行原生SQL来实现。

```c#
using (var transaction = context.Database.BeginTransaction())
{
    var student = context.Students.FromSqlRaw("SELECT * FROM Students WITH (UPDLOCK, ROWLOCK) WHERE ID = {0}", 1).First();

    student.Name = "小明";
    result = context.SaveChanges();
    transaction.Commit();
}
```

上面这段代码就是在EFCore中使用悲观锁的例子，这样每次取数据都要加锁的方式，不仅麻烦不说，而且性能差，还可能导致其他问题。

这时候就可以使用乐观锁，它可以允许多个用户相互独立进行更改数据，而不产生数据库锁。悲观锁每次读取数据都默认加锁，乐观锁每次读取数据都默认不加锁，只有发现数据将要被更新的时候再去判断当前数据的一致性。EFCore本身也支持乐观锁，只需要在可能产生并发冲突的模型类中添加一个byte[]类型的RowVersion属性，再给它添加上[Timestamp]特性。

```C#
public class Student
{
    public int ID { get; set; }

    public string Name { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}
```

如果想保持模型类干净的话，也可以使用Fluent API来定义rowversion令牌

```C#
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Student>()
                .Property(p => p.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
}
```

这样EFCore在生成更新语句的同时，就会根据RowVersion列去判断数据在被取出来的过程中有没有被更新。如果没有直接就是正常流程，如果有就报DbUpdateConcurrencyException错误，接着就进行逻辑处理（进行提示，放弃改动等等）。

下面演示一下发生并发冲突时的操作

```C#
using (var contextA = new AppDbContext())
{
    var stuA = contextA.Students.First(p => p.ID == 1);
    Console.WriteLine($"A 读取：Name={stuA.Name}, RowVersion={BitConverter.ToString(stuA.RowVersion)}");
    stuA.Name = "小明";	// A想把学生名字改为小明

    // B在A还没保存时读取
    using (var contextB = new AppDbContext())
    {
        var stuB = contextB.Students.First(p => p.ID == 1);
        Console.WriteLine($"B 读取：Name={stuB.Name}, RowVersion={BitConverter.ToString(stuB.RowVersion)}");
        stuB.Name = "小王";	// B想把学生名字改为小王
        dbB.SaveChanges();	// B进行保存
        Console.WriteLine("B保存成功");
    }

    // 现在A尝试保存
    try
    {
        dbA.SaveChanges();
        Console.WriteLine("A保存成功");		// 不应该执行到这一步，应该是保存错误，这是意外保存成功
    }
    catch (DbUpdateConcurrencyException ex)
    {
        Console.WriteLine("A保存失败，发生并发冲突");
        var entry = ex.Entries.First();
        var dbValue = entry.GetDatabaseValues(); // 读取当前数据库的最新值
        
        if (dbValue == null)
            Console.WriteLine("数据已被删除");
        else
        {
            Console.WriteLine($"数据库当前值Name={dbValue["Name"]}, RowVersion = {BitConverter.ToString((byte[])dbValue["RowVersion"])}");
            // 把实体信息还原为当前数据库值
            entry.OriginalValues.SetValues(databaseValues);	// 将实体快照更新为当前数据库值
			entry.CurrentValues.SetValues(databaseValues);  // 将实体信息同样更新为当前数据库值
            // 或者进行其他的逻辑处理操作
        }
    }
}
```

运行程序会发现，StudentName的值会是小王而不是小明，证明触发并发控制，A的更新操作被阻断。
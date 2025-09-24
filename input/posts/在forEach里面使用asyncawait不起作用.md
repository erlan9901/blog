Title : 在forEach里面使用async/await不起作用

Date : 2023-6-3

Tag : JavaScript

---

## JavaScript中在forEach里面使用async/await不起作用

假设有这样一段代码，需要去循环调用每个接口，然后等所有的异步操作完成后再进行下一步。这里我用Postman来模拟后端接口。

```javascript
const urls = ['https://c5f936e1-cc3f-42d6-af9d-050e1ae87ab0.mock.pstmn.io?Step=1', 'https://c5f936e1-cc3f-42d6-af9d-050e1ae87ab0.mock.pstmn.io?Step=2', 'https://c5f936e1-cc3f-42d6-af9d-050e1ae87ab0.mock.pstmn.io?Step=3'];

urls.forEach(async (url) => {
  const res = await fetch(url);
  const json = await res.json();
  console.log('Step' + json["Step"] + ': ' + json["Value"]);
})

console.log('全部完成');
```

上面这段代码看上去很正常，循环，依次调用同一接口，每次Step+1，全部完成后打印输出。

但是结果是：

```cmd
E:\Test>node index.js
全部完成
Step3: true
Step1: true
Step2: true
```

显然，"全部完成"比正常调用接口返回的结果要提前输出，可以发现forEach内部并没有"等"，await并没有生效，也就是异步操作放在了后台完成，外层的代码立刻就执行了。

原因是数组的forEach方法是同步对数组的每项调用回调，不会收集回调产生的Promise，换句话说，forEach本身不会去等待回调返回的Promise。还有一个原因是async函数的内部await只会暂停该函数的流程，但是外层的，也就是调用它的同步函数不会暂停。

如果需要按照数组顺序执行调用，并等到全部调用完毕后再执行之后的代码就要换成for of:

```javascript
const urls = ['https://c5f936e1-cc3f-42d6-af9d-050e1ae87ab0.mock.pstmn.io?Step=1', 'https://c5f936e1-cc3f-42d6-af9d-050e1ae87ab0.mock.pstmn.io?Step=2', 'https://c5f936e1-cc3f-42d6-af9d-050e1ae87ab0.mock.pstmn.io?Step=3'];

for (var url of urls){
  const res = await fetch(url);
  const json = await res.json();
  console.log('Step' + json["Step"] + ': ' + json["Value"]);
}

console.log('全部完成');
```

这样运行的结果就是我们希望得到的

```cmd
E:\Test>node index.js
Step1: true
Step2: true
Step3: true
全部完成
```

上面for of是一个接口一个接口的调用，也就是顺序执行的。如果想要并行执行可以换成Promise.all

```javascript
async function getAll(urls) {
  await Promise.all(
    urls.map(async url => {
      const res = await fetch(url);
      const json = await res.json();
      console.log('Step' + json["Step"] + ': ' + json["Value"]);
    })
  );
  console.log('全部完成');
}
getAll(urls);
```

这样运行的结果就是1，2，3步几乎一起被打印到控制台。

**总结**：forEach里面写async/await不起作用，是因为forEach不等待回调函数返回的Promise，如果需要等待完成并且顺序执行就用for of，并行执行就用Promise.all

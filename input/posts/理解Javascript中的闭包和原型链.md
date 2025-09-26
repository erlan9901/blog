Title : 理解Javascript中的闭包和原型链

Date : 2023-4-18

Tag : JavaScript

---

## 理解Javascript中的闭包和原型链

一、闭包

先说概念，闭包是指在一个函数里边创建的另一个函数，这个函数能够访问它的外部作用域，同时这个函数是封闭的，如果外部的函数不把它返回出去，那么这个函数就无法被访问。

简而言之，就是在一个函数里边再定义一个函数，这个内部的函数可以访问外部函数中定义的变量，即使这个函数被放到全局作用域里运行，它依然能够访问到在之前作用域里面的变量。举个例子说明：

```javascript
function makeCounter() {
  let count = 0;
  return {
    increment() {
      count++;
    },
    decrement() {
      count--;
    },
    value() {
      return count;
    }
  }
}
```

这段代码定义了一个makeCounter函数，makeCounter返回一个函数对象，里面有加、减和查看三个方法，这三个方法都可以访问外部作用域里面的count变量。例如，我们接收它的返回值，然后进行加减看看count值是否变化

```javascript
let counter = makeCounter();
counter.increment();
console.log(counter.value());  // 打印1
counter.increment();
console.log(counter.value());  // 打印2
counter.decrement();
console.log(counter.value());  // 打印1
```

即counter这个函数对象的闭包在它被创建的时候就已经定义好了。就好比一个武汉人在武汉出生，哪怕他到了北京、上海等地方，他照样会说武汉话方言。

如果闭包中的函数没有作为返回值返回，那么这个函数在外部就是不可访问的，还可以通过闭包来模拟私有方法。



二、原型链

JavaScript里面的对象继承是基于原型（prototype）的，而不是类。每个对象都有一个隐藏属性`[[Prototype]]`。当访问一个对象的属性时，如果找不到，就会沿着原型链向上查找，直到null为止。例如：

```javascript
function Person(name) {
  this.name = name;
}
Person.prototype.sayHi = function() {
  console.log("你好，我叫" + this.name);
}

let LiHua = new Person("李华");
LiHua.sayHi();	// 这里找不到sayHi方法，就会在原型上继续查找，然后打印出“你好，我叫李华”
```

使用原型的好处在于，如果一组属性应该出现在每一个实例上，那我们就可以复用它们，特别是那些重复的方法。

例如，我们要创建几个员工对象，每个员工都有name，age属性和一个sayHi方法：

```javascript
const emps = [
  { name: "张三", age: 20, sayHi() {console.log("你好，我叫" + this.name)} },
  { name: "李四", age: 24, sayHi() {console.log("你好，我叫" + this.name)} },
  { name: "王五", age: 28, sayHi() {console.log("你好，我叫" + this.name)} }
];
```

那么这里的sayHi方法就是重复的，我们就可以为这些员工对象创建一个构造函数，这个构造函数就是它们的原型：

```javascript
function Employee(name, age) {
  this.name = name;
  this.age = age;
}

// 这里只要使用Employee构造函数创建的对象都会具有sayHi方法
Employee.prototype.sayHi = function() {
  console.log("你好，我叫" + this.name);
}

// 这里就避免了重复代码
const emps = [new Employee("张三", 20), new Employee("李四", 24), new Employee("王五", 28)]
```

这里emps[0],emps[1],emps[2]三个员工的原型都是Employee构造函数，它们的[[Prototype]]属性指向Employee.prototype这个对象，然后这个对象被添加了一个sayHi方法。所以，当我们使用emps[0].sayHi()的时候，先会去emps[0]里面查找这个方法，找不到就继续通过原型链往上找，也就是访问[[Prototype]]属性，而[[Prototype]]属性又指向Employee.prototype，于是就在这里找到了sayHi方法。

闭包和原型链都体现出，函数才是Javascript的主力，对象可以表示为函数+原型的特性。

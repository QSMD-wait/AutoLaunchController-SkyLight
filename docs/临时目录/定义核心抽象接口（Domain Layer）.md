# 🌌 AutoLaunchController‑SkyLight  
# **Domain Layer 核心抽象接口定义指导文档（完整版）**

> 适用于：架构师、核心开发者、插件开发者  
> 目标：定义平台的核心抽象模型与行为契约  
> 作用：作为整个系统的“协议层”，确保扩展性、稳定性与可维护性

---

# 目录

1. Domain Layer 的定位与原则  
2. 核心抽象体系概览  
3. Trigger（触发器）  
4. TriggerEvent（触发事件）  
5. Condition（判断器）  
6. Action（执行器）  
7. ActionResult（执行结果）  
8. FlowNode（流程节点）  
9. ExecutionContext（执行上下文）  
10. SensitivityLevel（敏感级别）  
11. 模块元数据（Metadata）  
12. 插件化机制与接口设计关系  
13. 扩展性策略与未来兼容性  
14. 典型执行链路示例  
15. 总结与最佳实践  

---

# 1️⃣ Domain Layer 的定位与原则

Domain Layer 是整个 SkyLight 平台的“协议层”，它必须：

### ✔ 定义抽象（接口）  
Trigger / Condition / Action / FlowNode / Context / Event / Result

### ✔ 定义模型（数据结构）  
ExecutionContext / TriggerEvent / ActionResult / Metadata

### ✔ 定义契约（行为规范）  
每个模块必须遵守的行为边界

### ❌ 不做的事  
Domain 层不包含：

- 具体实现  
- 系统 API 调用  
- UI  
- Rule Engine  
- Flow Engine  
- 日志  
- 配置  

Domain 层必须**零依赖**外部库（除了基础 BCL）。

---

# 2️⃣ 核心抽象体系概览

整个 Domain 层的核心抽象关系如下：

```
Trigger → TriggerEvent → ExecutionContext → Condition → Action → ActionResult
                         ↓
                      FlowNode
```

每个模块都必须：

- 有唯一 Id  
- 有 Name  
- 有 Metadata（用于插件系统）  
- 遵守统一接口  

---

# 3️⃣ Trigger（触发器）

触发器是事件源，是自动化的入口。

---

## 📌 3.1 接口定义

```csharp
public interface ITrigger
{
    string Id { get; }
    string Name { get; }

    TriggerMetadata Metadata { get; }

    void Start();
    void Stop();

    event EventHandler<TriggerEvent> Triggered;
}
```

---

## 📌 3.2 设计解释（强解释）

### ① 为什么必须有 Start/Stop？
- Rule Engine 需要控制触发器生命周期  
- 避免触发器在后台无限运行  
- 支持“禁用规则”时自动停止触发器  

### ② 为什么必须有 Metadata？
- UI 自动生成配置界面  
- 插件系统自动识别模块类型  
- 安全系统识别敏感级别  

### ③ 为什么事件必须是 TriggerEvent？
- 保证事件结构统一  
- 便于 Flow Engine 读取上下文  
- 便于日志系统记录触发信息  

---

# 4️⃣ TriggerEvent（触发事件）

触发器产生的事件必须结构化。

---

## 📌 4.1 数据结构

```csharp
public record TriggerEvent(
    DateTime Timestamp,
    string TriggerId,
    string EventType,
    IReadOnlyDictionary<string, object>? Data = null
);
```

---

## 📌 4.2 强解释

### 为什么用 record？
- 不可变  
- 线程安全  
- 适合事件传递  

### 为什么包含 Data？
- 不同触发器的数据不同（进程名、窗口标题、文件路径等）  
- 统一结构便于扩展  

---

# 5️⃣ Condition（判断器）

判断器是布尔逻辑节点。

---

## 📌 5.1 接口定义

```csharp
public interface ICondition
{
    string Id { get; }
    string Name { get; }

    ConditionMetadata Metadata { get; }

    bool Evaluate(ExecutionContext context);
}
```

---

## 📌 5.2 强解释

### 为什么只返回 bool？
- 判断器必须保持纯逻辑  
- 不允许产生副作用  
- 不允许修改上下文  

### 为什么必须接收 ExecutionContext？
- 判断器可能需要读取触发事件数据  
- 可能需要读取流程变量  

---

# 6️⃣ Action（执行器）

执行器是系统的“输出”。

---

## 📌 6.1 接口定义

```csharp
public interface IAction
{
    string Id { get; }
    string Name { get; }

    ActionMetadata Metadata { get; }

    ActionResult Execute(ExecutionContext context);
}
```

---

## 📌 6.2 强解释

### 为什么必须返回 ActionResult？
- UI 需要展示执行结果  
- Flow Engine 需要根据结果决定下一步  
- 日志系统需要记录执行状态  

### 为什么 Action 可以修改 ExecutionContext？
- Action 可能产生输出（如用户输入）  
- Flow Engine 需要共享变量  

---

# 7️⃣ ActionResult（执行结果）

```csharp
public record ActionResult(
    bool Success,
    string? Message = null,
    IReadOnlyDictionary<string, object>? Data = null
);
```

---

## 📌 强解释

### 为什么 Success 是 bool？
- Flow Engine 需要判断是否继续执行  
- UI 需要展示成功/失败  

### 为什么 Data 是字典？
- Action 输出可能是任意结构  
- 统一结构便于扩展  

---

# 8️⃣ FlowNode（流程节点）

流程节点是流程编排的基础。

---

## 📌 8.1 接口定义

```csharp
public interface IFlowNode
{
    string Id { get; }
    string Name { get; }

    FlowNodeMetadata Metadata { get; }

    Task ExecuteAsync(ExecutionContext context);
}
```

---

## 📌 8.2 强解释

### 为什么是异步？
- 节点可能包含等待、延迟、用户交互  
- 避免阻塞主线程  

### 为什么不返回 ActionResult？
- FlowNode 是更高层的抽象  
- 可能包含多个 Action  
- 可能包含分支、循环  

---

# 9️⃣ ExecutionContext（执行上下文）

这是整个系统的“血液”。

---

## 📌 9.1 数据结构

```csharp
public class ExecutionContext
{
    public TriggerEvent? Event { get; init; }

    public IDictionary<string, object> Variables { get; } = new Dictionary<string, object>();

    public IDictionary<string, object> Environment { get; } = new Dictionary<string, object>();

    public CancellationToken CancellationToken { get; init; }
}
```

---

## 📌 9.2 强解释

### 为什么 Variables 是可变的？
- FlowNode 需要写入变量  
- Action 可能产生输出  

### 为什么 Environment 是只读环境？
- 存放系统信息（CPU、网络、窗口状态）  
- 不允许修改  

### 为什么包含 CancellationToken？
- Flow Engine 支持中断、暂停、取消  

---

# 🔟 SensitivityLevel（敏感级别）

```csharp
public enum SensitivityLevel
{
    Normal,
    Warning,
    HighRisk
}
```

---

## 📌 强解释

用于：

- UI 显示风险等级  
- 管理员模式控制  
- 外部配置导入时自动禁用高风险模块  

---

# 1️⃣1️⃣ 模块元数据（Metadata）

所有模块都必须有元数据。

---

## 📌 11.1 接口定义

```csharp
public interface IModuleMetadata
{
    string Name { get; }
    string Description { get; }
    Version Version { get; }
    SensitivityLevel Sensitivity { get; }
}
```

---

## 📌 11.2 强解释

### 为什么必须有 Metadata？
- 插件系统自动识别模块  
- UI 自动生成配置界面  
- 安全系统识别敏感级别  
- 配置迁移时识别高风险模块  

---

# 1️⃣2️⃣ 插件化机制与接口设计关系

插件系统依赖：

- 接口（ITrigger / IAction / ICondition / IFlowNode）  
- Metadata  
- 反射扫描  

插件加载流程：

```
扫描 DLL → 查找实现接口的类 → 读取 Metadata → 注册到系统
```

Domain 层的接口设计必须保证：

- 插件无需修改核心代码  
- 插件可独立开发  
- 插件可热插拔  

---

# 1️⃣3️⃣ 扩展性策略与未来兼容性

这套接口设计保证：

### ✔ 新增 Trigger 不需要修改 Rule Engine  
### ✔ 新增 Action 不需要修改 Flow Engine  
### ✔ 新增 FlowNode 不需要修改 UI  
### ✔ 新增 Condition 不需要修改配置系统  
### ✔ 新增敏感级别不需要修改插件系统  

这是“平台级”的扩展性。

---

# 1️⃣4️⃣ 典型执行链路示例

```
TimerTrigger → TriggerEvent
→ ExecutionContext
→ ConditionTree
→ ActionSequence
→ FlowNode（可选）
→ ActionResult
→ 日志系统
→ UI 通知
```

整个链路完全基于 Domain 层定义的抽象。

---

# 1️⃣5️⃣ 总结与最佳实践

Domain 层必须：

- **保持纯净**（无实现、无依赖）  
- **保持稳定**（接口不轻易变动）  
- **保持抽象**（不关心业务细节）  
- **保持扩展性**（插件友好）  

你现在拥有的是一套：

- 工程级  
- 平台级  
- 可扩展  
- 可维护  
- 可插件化  
- 可长期演进  

的 Domain Layer 抽象体系。

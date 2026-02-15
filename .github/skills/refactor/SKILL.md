---
name: refactor
description: 'Surgical code refactoring to improve maintainability without changing behavior. Covers extracting functions, renaming variables, breaking down god functions, improving type safety, eliminating code smells, and applying design patterns. Less drastic than repo-rebuilder; use for gradual improvements.'
license: MIT
---

# Refactor

## Overview

Improve code structure and readability without changing external behavior. Refactoring is gradual evolution, not revolution. Use this for improving existing code, not rewriting from scratch.

## When to Use

Use this skill when:

- Code is hard to understand or maintain
- Functions/classes are too large
- Code smells need addressing
- Adding features is difficult due to code structure
- User asks "clean up this code", "refactor this", "improve this"

---

## Refactoring Principles

### The Golden Rules

1. **Behavior is preserved** - Refactoring doesn't change what the code does, only how
2. **Small steps** - Make tiny changes, test after each
3. **Version control is your friend** - Commit before and after each safe state
4. **Tests are essential** - Without tests, you're not refactoring, you're editing
5. **One thing at a time** - Don't mix refactoring with feature changes

### When NOT to Refactor

```
- Code that works and won't change again (if it ain't broke...)
- Critical production code without tests (add tests first)
- When you're under a tight deadline
- "Just because" - need a clear purpose
```

---

## Common Code Smells & Fixes

### 1. Long Method/Function

```csharp
// BAD: 200-line function that does everything
public async Task<OrderResult> ProcessOrder(int orderId) {
    // 50 lines: fetch order
    // 30 lines: validate order
    // 40 lines: calculate pricing
    // 30 lines: update inventory
    // 20 lines: create shipment
    // 30 lines: send notifications
}

// GOOD: Broken into focused functions
public async Task<OrderResult> ProcessOrder(int orderId) {
    var order = await FetchOrder(orderId);
    ValidateOrder(order);
    var pricing = CalculatePricing(order);
    await UpdateInventory(order);
    var shipment = await CreateShipment(order);
    await SendNotifications(order, pricing, shipment);
    return new OrderResult { Order = order, Pricing = pricing, Shipment = shipment };
}
```

### 2. Duplicated Code

```csharp
// BAD: Same logic in multiple places
public decimal CalculateUserDiscount(User user) {
    if (user.Membership == "gold") return user.Total * 0.2m;
    if (user.Membership == "silver") return user.Total * 0.1m;
    return 0;
}

public decimal CalculateOrderDiscount(Order order) {
    if (order.User.Membership == "gold") return order.Total * 0.2m;
    if (order.User.Membership == "silver") return order.Total * 0.1m;
    return 0;
}

// GOOD: Extract common logic
public decimal GetMembershipDiscountRate(string membership) {
    return membership switch {
        "gold" => 0.2m,
        "silver" => 0.1m,
        _ => 0m
    };
}

public decimal CalculateUserDiscount(User user) {
    return user.Total * GetMembershipDiscountRate(user.Membership);
}

public decimal CalculateOrderDiscount(Order order) {
    return order.Total * GetMembershipDiscountRate(order.User.Membership);
}
```

### 3. Large Class/Module

```csharp
// BAD: God object that knows too much
public class UserManager {
    public void CreateUser() { /* ... */ }
    public void UpdateUser() { /* ... */ }
    public void DeleteUser() { /* ... */ }
    public void SendEmail() { /* ... */ }
    public void GenerateReport() { /* ... */ }
    public void HandlePayment() { /* ... */ }
    public void ValidateAddress() { /* ... */ }
    // 50 more methods...
}

// GOOD: Single responsibility per class
public class UserService {
    public void Create(UserData data) { /* ... */ }
    public void Update(int id, UserData data) { /* ... */ }
    public void Delete(int id) { /* ... */ }
}

public class EmailService {
    public void Send(string to, string subject, string body) { /* ... */ }
}

public class ReportService {
    public void Generate(ReportType type, ReportParams parameters) { /* ... */ }
}

public class PaymentService {
    public void Process(decimal amount, PaymentMethod method) { /* ... */ }
}
```

### 4. Long Parameter List

```csharp
// BAD: Too many parameters
public void CreateUser(string email, string password, string name, int age, 
    string address, string city, string country, string phone) {
    /* ... */
}

// GOOD: Group related parameters
public record UserData(
    string Email,
    string Password,
    string Name,
    int? Age = null,
    Address? Address = null,
    string? Phone = null
);

public void CreateUser(UserData data) {
    /* ... */
}

// EVEN BETTER: Use builder pattern for complex construction
var user = new UserBuilder()
    .WithEmail("test@example.com")
    .WithPassword("secure123")
    .WithName("Test User")
    .WithAddress(address)
    .Build();
```

### 5. Magic Numbers/Strings

```csharp
// BAD: Unexplained values
if (user.Status == 2) { /* ... */ }
var discount = total * 0.15m;
Thread.Sleep(86400000);

// GOOD: Named constants
public enum UserStatus {
    Active = 1,
    Inactive = 2,
    Suspended = 3
}

public static class DiscountRates {
    public const decimal Standard = 0.1m;
    public const decimal Premium = 0.15m;
    public const decimal Vip = 0.2m;
}

public static class TimeSpans {
    public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
}

if (user.Status == UserStatus.Inactive) { /* ... */ }
var discount = total * DiscountRates.Premium;
Thread.Sleep(TimeSpans.OneDay);
```

### 6. Nested Conditionals

```csharp
// BAD: Arrow code
public Result Process(Order order) {
    if (order != null) {
        if (order.User != null) {
            if (order.User.IsActive) {
                if (order.Total > 0) {
                    return ProcessOrder(order);
                } else {
                    return Result.Error("Invalid total");
                }
            } else {
                return Result.Error("User inactive");
            }
        } else {
            return Result.Error("No user");
        }
    } else {
        return Result.Error("No order");
    }
}

// GOOD: Guard clauses / early returns
public Result Process(Order order) {
    if (order is null) return Result.Error("No order");
    if (order.User is null) return Result.Error("No user");
    if (!order.User.IsActive) return Result.Error("User inactive");
    if (order.Total <= 0) return Result.Error("Invalid total");
    
    return ProcessOrder(order);
}
```

### 7. Dead Code

```csharp
// BAD: Unused code lingers
public void OldImplementation() { /* ... */ }
private const int DEPRECATED_VALUE = 5;
// Commented out code
// public void OldCode() { /* ... */ }

// GOOD: Remove it
// Delete unused functions, imports, and commented code
// If you need it again, git history has it
```

---

## Refactoring Steps

### Safe Refactoring Process

```
1. PREPARE
   - Ensure tests exist (write them if missing)
   - Commit current state
   - Create feature branch

2. IDENTIFY
   - Find the code smell to address
   - Understand what the code does
   - Plan the refactoring

3. REFACTOR (small steps)
   - Make one small change
   - Run tests
   - Commit if tests pass
   - Repeat

4. VERIFY
   - All tests pass
   - Manual testing if needed
   - Performance unchanged or improved

5. CLEAN UP
   - Update comments
   - Update documentation
   - Final commit
```

---

## Refactoring Checklist

### Code Quality

- [ ] Functions are small (< 50 lines)
- [ ] Functions do one thing
- [ ] No duplicated code
- [ ] Descriptive names (variables, functions, classes)
- [ ] No magic numbers/strings
- [ ] Dead code removed

### Structure

- [ ] Related code is together
- [ ] Clear module boundaries
- [ ] Dependencies flow in one direction
- [ ] No circular dependencies

### Type Safety

- [ ] Types defined for all public APIs
- [ ] Nullable reference types properly annotated
- [ ] No unnecessary casts

### Testing

- [ ] Refactored code is tested
- [ ] Tests cover edge cases
- [ ] All tests pass

---

## Common Refactoring Operations

| Operation                                    | Description                           |
| -------------------------------------------- | ------------------------------------- |
| Extract Method                               | Turn code fragment into method        |
| Extract Class                                | Move behavior to new class            |
| Extract Interface                            | Create interface from implementation  |
| Inline Method                                | Move method body back to caller       |
| Inline Class                                 | Move class behavior to caller         |
| Pull Up Method                               | Move method to base class             |
| Push Down Method                             | Move method to derived class          |
| Rename Method/Variable                       | Improve clarity                       |
| Introduce Parameter Object                   | Group related parameters              |
| Replace Conditional with Polymorphism        | Use polymorphism instead of switch/if |
| Replace Magic Number with Constant           | Named constants                       |
| Decompose Conditional                        | Break complex conditions              |
| Consolidate Conditional                      | Combine duplicate conditions          |
| Replace Nested Conditional with Guard Clauses| Early returns                         |
| Replace Type Code with Enum                  | Strong typing                         |
| Replace Inheritance with Composition         | Composition over inheritance          |

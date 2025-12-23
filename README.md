# AutoTestRunner

**AutoTestRunner** is a desktop application designed for convenient execution of automated tests from `.dll` assemblies.  
The application automatically loads tests, builds a hierarchical tree of test scenarios, allows you to run selected tests, configure the number of iterations, and generates detailed reports using **ExtentReports**.

---

## 🚀 Key Features

### ✔ Automatic Test Loading from DLL
- Supports NUnit / xUnit / MSTest (depending on implementation).
- Test method discovery using Reflection.
- Hierarchical tree structure:
  **Assembly → Namespace → Class → Test Methods**.

### ✔ Interactive User Interface
- Convenient tree view of all tests.
- Select one or multiple tests to run.
- Status highlighting (Passed / Failed / Skipped).

### ✔ Run Configuration
- Launch a single test or groups of tests.
- Multiple iterations for stability checks.
- Sequential execution supported.

### ✔ Detailed ExtentReports HTML Reports
- Beautiful reports with logs and step details.
- Automatically generated after each test run.
- Maintains report history.

### ✔ Asynchronous Execution
- The UI remains responsive.
- Progress bar for current run.
- Execution summary collected at the end.

### ✔ Logging
- Supports NLog / Serilog.
- Logs for assembly loading, test execution, and errors.

---

## 🧩 Architecture

```

AutoTestRunner
├── UI (WPF / WinUI)
│    ├── TestTreeView
│    ├── RunPanel
│    └── SettingsPage
├── TestLoader
│    └── Reflection / TestFrameworkAdapters
├── TestRunner
│    └── Executor (async)
├── ReportManager
│    └── ExtentReports generator
└── Logging

```

---

## 🛠 Usage

### 1. Load a DLL  
Menu → **File → Load Test Assembly**

After loading, the test tree will appear.

### 2. Select Tests  
Check one or more tests in the tree.

### 3. Set the Run Count  
Use the **Run Count** field.

### 4. Start Execution  
Press **Run Tests**.

### 5. View the Report  
The ExtentReport will be generated in:

```

/Reports/<timestamp>/index.html

```

---

## 🔧 Roadmap

- Plugin support for different test frameworks  
- Parallel execution  
- Remote execution API  
- CI/CD integration (Azure DevOps, GitHub Actions)

---

## 📜 License

MIT License.

---

# Error Logging Guide for WPF Application

## Overview

By following this guide, you will set up error logging in your WPF application. This will dump errors inside a a MessageBox and a trace file located in the "Logs" folder of your application before crashing.

---

## Step 1: Modify App.xaml

Add the following property to the `<Application>` tag in `App.xaml`:

<Application
    ...
    DispatcherUnhandledException="App_OnDispatcherUnhandledException">
</Application>

## Step 2: Implement Exception Handling in App.xaml.cs

Add the following function to `App.xaml.cs`:

private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
{
    string exceptionMessageText = $"An exception occurred: {e.Exception.Message}\r\n\r\nat: {e.Exception.StackTrace}";
    LoggingService.Log(e.Exception);
    // Create a Window to display the exception information.
    MessageBox.Show(exceptionMessageText, "Unhandled Exception", MessageBoxButton.OK);
}

## Step 3: Add Required Usings

Add these namespaces to `App.xaml.cs`:

using NITHlibrary.ErrorLogging;
using System.Windows.Threading;

## Step 4: Initialize Trace in Main Window

Call the `TraceAdder.AddTrace()` method in the constructor of your Main Window, just below the `InitializeComponent()` call:

public MainWindow()
{
    InitializeComponent();
    TraceAdder.AddTrace();
}

## Step 5: Add Required Using in Main Window

Add this namespace to your Main Window class:

using NITHlibrary.ErrorLogging;

---

You can test the setup by throwing a random exception (e.g., `throw new Exception("Test");`) at some point in your code, such as after a button press.
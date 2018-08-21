using System;
using System.Activities;
using System.Collections.Generic;
using System.Threading;

namespace WorkflowDemo
{

    class Program
    {
        enum 工作流类型 { 顺序工作流, 流程图工作流, 状态机工作流 }

        static Activity 获取工作流对象(工作流类型 type)
        {
            switch (type)
            {
                case 工作流类型.顺序工作流:
                    return new SequentialNumberGuessWorkflow();
                case 工作流类型.流程图工作流:
                    return new FlowchartNumberGuessWorkflow();
                case 工作流类型.状态机工作流:
                    return new StateMachineNumberGuessWorkflow();
                default:
                    // 默认为顺序工作流
                    return new FlowchartNumberGuessWorkflow();
            }
        }

        static void Main(string[] args)
        {
            AutoResetEvent syncEvent = new AutoResetEvent(false);
            AutoResetEvent idleEvent = new AutoResetEvent(false);

            var inputs = new Dictionary<string, object>() { { "MaxNumber", 100 } };

            Activity activity = 获取工作流对象(工作流类型.状态机工作流);

            WorkflowApplication wfApp = new WorkflowApplication(activity, inputs);

            wfApp.Completed = delegate (WorkflowApplicationCompletedEventArgs e)
            {
                int Turns = Convert.ToInt32(e.Outputs["Turns"]);
                Console.WriteLine("恭喜，你猜对了 {0} 转的数字。", Turns);

                syncEvent.Set();
            };

            wfApp.Aborted = delegate (WorkflowApplicationAbortedEventArgs e)
            {
                Console.WriteLine(e.Reason);
                syncEvent.Set();
            };

            wfApp.OnUnhandledException = delegate (WorkflowApplicationUnhandledExceptionEventArgs e)
            {
                Console.WriteLine(e.UnhandledException.ToString());
                return UnhandledExceptionAction.Terminate;
            };

            wfApp.Idle = delegate (WorkflowApplicationIdleEventArgs e)
            {
                idleEvent.Set();
            };

            wfApp.Run();

            // 循环直到工作流程完成。
            WaitHandle[] handles = new WaitHandle[] { syncEvent, idleEvent };
            while (WaitHandle.WaitAny(handles) != 0)
            {
                // 收集用户输入并恢复书签。
                bool validEntry = false;
                Console.WriteLine("请输入一个整数。");
                while (!validEntry)
                {
                    int Guess;
                    if (!Int32.TryParse(Console.ReadLine(), out Guess))
                    {
                        Console.WriteLine("请输入一个整数。");
                    }
                    else
                    {
                        validEntry = true;
                        // 把输入值传递给工作流，第一个参数为属性名，第二个为值
                        wfApp.ResumeBookmark("EnterGuess", Guess);
                    }
                }
            }
        }

    }
}

using System;
using System.Activities;

namespace WorkflowDemo
{

    public sealed class ReadInt : NativeActivity<int>
    {
        // 定义一个字符串类型的活动输入参数
        [RequiredArgument]
        public InArgument<string> BookmarkName { get; set; }

        // 如果活动返回值，则从 NativeActivityContext<TResult>
        // 并从 Execute 方法返回该值
        protected override void Execute(NativeActivityContext context)
        {
            string name = BookmarkName.Get(context);

            if (name == string.Empty)
            {
                throw new ArgumentException("BookmarkName不能是空字符串。",
                    "BookmarkName");
            }

            context.CreateBookmark(name, new BookmarkCallback(OnReadComplete));
        }


        //NativeActivity派生的活动通过调用执行脱机操作
        //System.Activities.NativeActivityContext上定义的CreateBookmark重载之一
        //必须覆盖CanInduceIdle属性并返回true。
        protected override bool CanInduceIdle
        {
            get { return true; }
        }

        void OnReadComplete(NativeActivityContext context, Bookmark bookmark, object state)
        {
            this.Result.Set(context, Convert.ToInt32(state));
        }
    }
}

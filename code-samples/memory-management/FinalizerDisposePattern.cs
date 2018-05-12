namespace BanksySan.Workshops.AdvancedCSharp.MemoryManagement
{
    using System;
    using static System.Console;
    
    public class CorrectDisposeFinalizeType : IDisposable 
    {  
        protected virtual void Dispose(bool disposing)
        {
            WriteLine($"Dispose called with disposing = {disposing}.");
            WriteLine("Release unmanaged, non-disposable objects");
            if (disposing)
                WriteLine("Disposing of other IDisposable objects");
        }  

        ~ CorrectDisposeFinalizeType(){
            WriteLine("Finalizer call.");
            Dispose(false);  
        }  

        public void Dispose()
        {
            WriteLine("Called IDisposable.Dispose().");
            Dispose(true);  
            WriteLine("Suppressing finalize.");
            GC.SuppressFinalize(this);  
        }  
    }

    static class FinalizerDisposePattern
    {
        private static void Main()
        {
            WriteLine("Wrapped with using:");
            using (var correctDisposeFinalizeType = new CorrectDisposeFinalizeType())
            {
                GC.KeepAlive(correctDisposeFinalizeType);
            }
            WriteLine("Not wrapped with using:");
            var o2 = new CorrectDisposeFinalizeType();
        }
    }
}
namespace AutoBLv2
{
    internal class Program
    {
        #region Constants
        //-----------------------------------------
        private const Int32 SERVER_PORT = 13102;
        //-----------------------------------------        
        #endregion


        //=========================================
        static void Main(string[] args)
        {
            Console.WriteLine("AutoBlv2 Server Start");
            Server server = new Server(SERVER_PORT);
        }
        //=========================================
    }
}

using AutoClickerController;

public class Entry{
[STAThread]
    public static void Main(string[] args){
        try{
            Controller controller = new Controller();
            controller.Run();
        } catch (Exception e){
            MessageBox.Show(e.Message, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
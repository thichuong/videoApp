using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace videoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra thông tin đăng nhập
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Thực hiện kiểm tra tài khoản và mật khẩu ở đây.
            // Đây chỉ là ví dụ đơn giản, bạn cần thêm logic phức tạp hơn để xử lý đăng nhập.

            if (username == "x" && password == "lex@x")
            {
                EditVideoWindow editVideoWindow = new EditVideoWindow();
                editVideoWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại. Vui lòng kiểm tra lại tài khoản và mật khẩu.");
            }
        }
    }
}

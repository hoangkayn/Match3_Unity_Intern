- Thay đổi lại toàn bộ skin trong game
- Khi tạo board ban đầu, sẽ luôn có tất cả các loại item và số lương mỗi loại đều chia hết cho 3
- Tạo hàng dưới cùng(bottomrow) có 5 cell
- Hiển thị màn hình chiến thắng và thua cuộc khi chơi
- Tạo nút AutoWin sẽ tự động phát cho đến khi thắng, mỗi hành động có độ trễ 0.5s (Cơ chế là tạo 1 danh sách các item ở trong board để so sách với các item đang có ở bottomrow,ưu tiên xếp các item có trong bottomrow và số lượng ở trong bottomrow lớn nhất để nhanh chóng tạo match)
- Tạo nút AutoLose sẽ tự đông phát cho đến khi thua, mỗi hành động có độ trễ 0.5s(Cơ chế là tạo 1 danh sách các item ở trong board để so sách với các item đang có ở bottomrow, ưu tiên xếp các item có số lượng trong bottomrow thấp nhất để khó tạo match)
- Thêm hoạt ảnh khi một item di chuyển từ board vào bottomrow, item sẽ ưu tiên di chuyển đến gần các item giống nó ở bottomrow
- Thêm hoạt ảnh khi các item giống nhau bị xoá (scale trở thành 0)
- Thêm hoạt ảnh dịch sang trái (khi các item giống nhau bị xoá, các item còn lại sẽ dịch sang trái)
- Thêm hoạt ảnh dịch sang phải (khi  item mới di chuyển xuống bottomrow, nếu ở bottomrow có item cùng loại thì nó sẽ làm dịch chuyển các item khác ở bên phải để chiếm chỗ )
- Thêm nút Time Attack, tạo chế độ chơi theo thời gian, người chơi sẽ không thua khi các ô ở bottomrow đã đầy, người chơi có thể đưa 1 ô từ bottomrow trở về vị trí ban đầu ở board bằng cách chạm vào nó, người chơi sẽ thua nếu không xoá hết item trong 1 phút.
- Cơ chế Pause hoạt động bình thường ở chế độ auto hoặc thường, khi game đang auto ngoài pause game không thực hiện được hành động nào khác.
- Các item đang di chuyển có layer nằm trên các item khác.



document.getElementById("add-to-cart").addEventListener("click", function () {
    // Lấy số lượng sản phẩm hiện tại
    var numElement = document.querySelector(".cart .num");
    var num = parseInt(numElement.textContent);

    // Tăng số lượng lên 1
    num += 1;

    // Cập nhật lại số lượng trên giao diện
    numElement.textContent = num;
});
using System.Linq;
using System.Web.Mvc;
using Warehouse_25DH110779.Models;

namespace Warehouse_25DH110779.Controllers
{
    public class ProductsController : Controller
    {
        private readonly Warehouse_25DH110779Entities db =
            new Warehouse_25DH110779Entities();

        public ActionResult Index()
        {
            var products = db.Products
                .OrderBy(p => p.ProductID)
                .ToList();

            return View(products);
        }

        [HttpGet]
        public ActionResult Filter(
     string category = null,
     int? minQuantity = null,
     int? maxQuantity = null,
     string sort = "asc")
        {
            ViewBag.Title = "Lọc và sắp xếp tồn kho";

            var categories = db.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Categories = new SelectList(categories, category);

            ViewBag.SortOptions = new SelectList(
                new[]
                {
            new { Value = "asc", Text = "Số lượng tăng dần" },
            new { Value = "desc", Text = "Số lượng giảm dần" }
                },
                "Value", "Text", sort);

            if ((minQuantity.HasValue && minQuantity.Value < 0) ||
                (maxQuantity.HasValue && maxQuantity.Value < 0))
            {
                ModelState.AddModelError(
                    "", "Số lượng không được âm.");
            }

            if (minQuantity.HasValue && maxQuantity.HasValue &&
                minQuantity.Value > maxQuantity.Value)
            {
                ModelState.AddModelError(
                    "", "Từ số lượng phải nhỏ hơn hoặc bằng Đến số lượng.");
            }

            if (!ModelState.IsValid)
                return View(Enumerable.Empty<Products>());

            var query = db.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            if (minQuantity.HasValue)
                query = query.Where(p => p.Quantity >= minQuantity.Value);

            if (maxQuantity.HasValue)
                query = query.Where(p => p.Quantity <= maxQuantity.Value);

            var products = sort == "desc"
                ? query.OrderByDescending(p => p.Quantity)
                       .ThenBy(p => p.ProductID).ToList()
                : query.OrderBy(p => p.Quantity)
                       .ThenBy(p => p.ProductID).ToList();

            return View(products);
        }

        [HttpGet]
        public ActionResult Receive()
        {
            return StockForm("Receive");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Receive(int? productId, int? quantity)
        {
            return ChangeStock(true, productId, quantity);
        }

        [HttpGet]
        public ActionResult Damage()
        {
            return StockForm("Damage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Damage(int? productId, int? quantity)
        {
            return ChangeStock(false, productId, quantity);
        }

        private ActionResult StockForm(string action, int? productId = null)
        {
            ViewBag.Title = action == "Receive"
                ? "Nhập kho" : "Ghi nhận hàng hỏng";

            ViewBag.FormAction = action;

            var products = db.Products
                .OrderBy(p => p.ProductID)
                .ToList()
                .Select(p => new
                {
                    p.ProductID,
                    Label = p.ProductID + " - " + p.ProductName
                        + " | Tồn: " + p.Quantity
                });

            ViewBag.ProductList = new SelectList(
                products, "ProductID", "Label", productId);

            return View("Stock");
        }

        private ActionResult ChangeStock(
            bool isReceive, int? productId, int? quantity)
        {
            string action = isReceive ? "Receive" : "Damage";

            if (!productId.HasValue)
                ModelState.AddModelError("productId", "Hãy chọn sản phẩm.");

            if (!quantity.HasValue || quantity.Value <= 0)
                ModelState.AddModelError(
                    "quantity", "Số lượng phải là số nguyên dương.");

            if (!ModelState.IsValid)
                return StockForm(action, productId);

            using (var transaction = db.Database.BeginTransaction(
                System.Data.IsolationLevel.Serializable))
            {
                var p = db.Products.Find(productId.Value);

                if (p == null)
                {
                    ModelState.AddModelError("", "Sản phẩm không tồn tại.");
                    return StockForm(action, productId);
                }

                int oldQuantity = p.Quantity;

                if (!isReceive && quantity.Value > oldQuantity)
                {
                    ModelState.AddModelError(
                        "", "Số hàng hỏng vượt tồn kho. Tồn hiện tại: "
                            + oldQuantity);

                    return StockForm(action, productId);
                }

                if (isReceive &&
                    quantity.Value > int.MaxValue - oldQuantity)
                {
                    ModelState.AddModelError(
                        "", "Số lượng sau khi nhập vượt giới hạn.");

                    return StockForm(action, productId);
                }

                p.Quantity = isReceive
                    ? oldQuantity + quantity.Value
                    : oldQuantity - quantity.Value;

                db.SaveChanges();
                transaction.Commit();

                TempData["Message"] =
                    (isReceive ? "Nhập kho" : "Ghi nhận hàng hỏng")
                    + " - Mã " + p.ProductID + ": "
                    + oldQuantity
                    + (isReceive ? " + " : " - ")
                    + quantity.Value + " = " + p.Quantity;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Transfer()
        {
            return Pending("Điều chuyển hàng");
        }

        public ActionResult Create()
        {
            return Pending("Thêm sản phẩm");
        }

        public ActionResult Statistics()
        {
            return Pending("Thống kê tồn kho");
        }

        private ActionResult Pending(string title)
        {
            ViewBag.Title = title;
            return View("Pending");
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction("Index");

            var p = db.Products.Find(id.Value);

            if (p == null)
                return HttpNotFound("Sản phẩm không tồn tại.");

            if (p.Quantity != 0)
            {
                TempData["Message"] =
                    "Không thể xóa sản phẩm còn hàng.";

                return RedirectToAction("Index");
            }

            return View(p);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (!id.HasValue)
            {
                TempData["Message"] =
                    "Chưa chọn sản phẩm cần xóa.";

                return RedirectToAction("Index");
            }

            using (var transaction = db.Database.BeginTransaction(
                System.Data.IsolationLevel.Serializable))
            {
                var p = db.Products.Find(id.Value);

                if (p == null)
                {
                    TempData["Message"] =
                        "Sản phẩm không tồn tại hoặc đã bị xóa.";

                    return RedirectToAction("Index");
                }

                if (p.Quantity != 0)
                {
                    TempData["Message"] =
                        "Không thể xóa sản phẩm còn hàng.";

                    return RedirectToAction("Index");
                }

                db.Products.Remove(p);
                db.SaveChanges();
                transaction.Commit();

                TempData["Message"] =
                    "Đã xóa sản phẩm hết hàng.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
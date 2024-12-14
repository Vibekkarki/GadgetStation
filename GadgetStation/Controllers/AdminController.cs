using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GadgetStation.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

public class AdminController : Controller
{
    private readonly GadgetStationDbContextcs _context;

    public AdminController(GadgetStationDbContextcs context)
    {
        _context = context;
    }

    // GET: Admin/Index
    public async Task<IActionResult> Index(int productPage = 1, int userPage = 1, int orderPage = 1, int pageSize = 5)
    {
        // Paginate products
        var products = await _context.Products
            .Include(p => p.Category)
            .Skip((productPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var productCount = await _context.Products.CountAsync();

        // Paginate users
        var users = await _context.Users
            .Skip((userPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userCount = await _context.Users.CountAsync();

        // Paginate orders
        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Skip((orderPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var orderCount = await _context.Orders.CountAsync();

        // Create a view model
        var viewModel = new AdminIndexViewModel
        {
            Products = products,
            Users = users,
            Orders = orders,
            ProductPagination = new PaginationModel
            {
                CurrentPage = productPage,
                TotalItems = productCount,
                PageSize = pageSize
            },
            UserPagination = new PaginationModel
            {
                CurrentPage = userPage,
                TotalItems = userCount,
                PageSize = pageSize
            },
            OrderPagination = new PaginationModel
            {
                CurrentPage = orderPage,
                TotalItems = orderCount,
                PageSize = pageSize
            }
        };

        return View(viewModel);
    }



    // GET: Admin/ManageProducts
    public async Task<IActionResult> ManageProducts(int page = 1, int pageSize = 10)
    {
        var products = _context.Products.Include(p => p.Category);

        // Pagination logic
        int totalProducts = await products.CountAsync();
        var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

        // Get paginated data
        var paginatedProducts = await products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Pass pagination details via ViewData
        ViewData["TotalPages"] = totalPages;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;

        return View(paginatedProducts);
    }



    // GET: Admin/ManageUsers
    public async Task<IActionResult> ManageUsers(int page = 1, int pageSize = 10)
    {
        // Calculate the total number of users in the database
        var totalUsers = await _context.Users.CountAsync();

        // Fetch the users for the current page with pagination
        var users = await _context.Users
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

        // Create a view model or pass the necessary data to the view
        ViewData["TotalPages"] = totalPages;
        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;

        return View(users);
    }

    // GET: Admin/CreateProduct
    public IActionResult CreateProduct()
    {
        ViewData["Categories"] = _context.Categories.ToList();
        return View();
    }

    // POST: Admin/CreateProduct
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct([Bind("ProductId,Name,Description,Price,Stock,CategoryId,ImageUrl")] Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessageProduct"] = "Product Added successfully.";
            return RedirectToAction(nameof(ManageProducts));
        }
        ViewData["Categories"] = _context.Categories.ToList();

        return View(product);
    }

    // GET: Admin/CreateUser
    public IActionResult CreateUser()
    {
        return View();
    }

    // POST: Admin/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser([Bind("UserId,Name,Email,Password,Role")] User user)
    {
        if (ModelState.IsValid)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (existingUser != null)
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }
            _context.Add(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessageUser"] = "User Created successfully.";
            return RedirectToAction(nameof(ManageUsers));
        }
        return View(user);
    }

    // GET: Admin/EditProduct/5
    public async Task<IActionResult> EditProduct(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        ViewData["Categories"] = _context.Categories.ToList();
        return View(product);
    }

    // POST: Admin/EditProduct/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, [Bind("ProductId,Name,Description,Price,Stock,CategoryId,ImageUrl")] Product product)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["SuccessMessage"] = "Product [" + product.Name.ToString() + "] Edited successfully.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
        return View(product);
    }

    // POST: Admin/DeleteProduct/5
    [HttpPost, ActionName("DeleteProduct")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProductConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Product deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/EditUser/5
    public async Task<IActionResult> EditUser(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: Admin/EditUser/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, [Bind("UserId,Name,Email,Password,Role")] User user)
    {
        if (id != user.UserId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.UserId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["SuccessMessage"] = "User [" + user.Name.ToString() + "] Edited successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // POST: Admin/DeleteUser/5fv
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }


    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.ProductId == id);
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.UserId == id);
    }
}

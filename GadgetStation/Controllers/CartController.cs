using GadgetStation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GadgetStation.Controllers
{
    public class CartController : Controller
    {
        private readonly GadgetStationDbContextcs _context;

        public CartController(GadgetStationDbContextcs context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = User.Identity.Name;
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = User.Identity.Name;
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                _context.Update(existingItem);
            }
            else
            {
                var newCartItem = new CartItem
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1
                };
                _context.CartItems.Add(newCartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            // Check if the quantity exceeds the limit
            if (quantity > 10)
            {
                TempData["ErrorCart"] = "Quantity cannot exceed 10 per item.";
                return RedirectToAction("Index", "Cart");
            }

            // Retrieve the cart item from the database
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(item => item.CartItemId == cartItemId);

            if (cartItem != null)
            {
                // Update the quantity of the cart item
                cartItem.Quantity = quantity;

                // Save changes to the database
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();

                // Success message
                TempData["SuccessCart"] = "Cart item updated successfully!";
            }

            // Redirect back to the cart page with the updated values
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessCart"] = "Cart item Deleted successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = User.Identity.Name;
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCheckout(Order order)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = User.Identity.Name;
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            // Check if cart items are empty
            if (!cartItems.Any())
            {
                return RedirectToAction("Index");
            }

            // Set order details
            order.UserId = userId;
            order.TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price);
            order.OrderDate = DateTime.UtcNow;

            // Add the order to the Orders table
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Ensure that the OrderId is generated
            if (order.OrderId == 0)
            {
                // Log or handle the case where the order wasn't saved correctly
                return View("Error", new { message = "Order wasn't saved correctly." });
            }

            //  add the OrderDetails
            foreach (var cartItem in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.Product.Price
                };

                _context.OrderDetails.Add(orderDetail);
            }

            await _context.SaveChangesAsync(); // Save the order details

            // Ensure the OrderDetails were added
            if (!_context.OrderDetails.Any(od => od.OrderId == order.OrderId))
            {
                // Log or handle the case where order details weren't saved correctly
                return View("Error", new { message = "Order details weren't saved correctly." });
            }

            // clear the cart items now that the order is placed
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // Redirect to the Thank You page
            return RedirectToAction("ThankYou");
        }



        public IActionResult ThankYou()
        {
            return View();
        }
    }
}

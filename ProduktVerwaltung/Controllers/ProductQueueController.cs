using System.Drawing;
using System.Text;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Produktverwaltung.Services;
using ProduktVerwaltung.Models;
using ProduktVerwaltung.Services;

namespace ProduktVerwaltung.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProductQueueController : ControllerBase
    {
        private readonly ProductContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductItemsController> _logger;

        public ProductQueueController(
            ProductContext context,
            ILogger<ProductItemsController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: api/ProductQueue
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductItem>>> GetProductItems()
        {
          if (_context.ProductItems == null)
          {
              return NotFound();
          }
            return await _context.ProductItems.ToListAsync();
        }

        // GET: api/ProductQueue/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductItem>> GetProductItem(long id)
        {
          if (_context.ProductItems == null)
          {
              return NotFound();
          }
            var productItem = await _context.ProductItems.FindAsync(id);

            if (productItem == null)
            {
                return NotFound();
            }

            return productItem;
        }

        // PUT: api/ProductQueue/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductItem(long id, ProductItem productItem)
        {
            if (id != productItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(productItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProductQueue
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductItem>> PostProductItem(ProductItem productItem)
        {
          if (_context.ProductItems == null)
          {
              return Problem("Entity set 'ProductContext.ProductItems'  is null.");
          }

            _context.ProductItems.Add(productItem);
            await _context.SaveChangesAsync();

            //dummy bitmap
            //Bitmap img = new Bitmap(1, 1);
            Bitmap img = new(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            // The font for our text
            Font f = new Font("Arial", 14);

            // work out how big the text will be when drawn as an image
            SizeF size = drawing.MeasureString(productItem.Name, f);

            // create a new Bitmap of the required size
            img = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
            drawing = Graphics.FromImage(img);

            // give it a white background
            drawing.Clear(Color.White);

            // draw the text in black
            drawing.DrawString(productItem.Name, f, Brushes.Black, 0, 0);

            String _pictureName = productItem.Name + ".jpg";
            img.Save(@$".\{_pictureName}");
            drawing.Save();

            //Save into Blob
            BlobService blobService = new BlobService(_configuration);
            blobService.UploadDataToBlobContainer(Environment.CurrentDirectory, productItem.Name, "imageblob");

            String _message = productItem.Id + " " + productItem.Bezeichnung + ' ' + _pictureName + ' ' + productItem.Preis;

            //List<char> charsToRemove = new List<char>() { '@', '_', ',', '.' };

            //byte[] buffer = new byte[_message.Length];
            //String _message_base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(_message));

            //String encodedString = Base64.getEncoder().encodeToString(originalInput.getBytes());
            //String res = DatatypeConverter.printBase64Binary(str.getBytes());

            QueueService queueService = new QueueService(_configuration);
            queueService.CreateQueue("queue-produkt-verwaltung");
            queueService.InsertMessage("queue-produkt-verwaltung", Newtonsoft.Json.JsonConvert.SerializeObject(_message));

            return CreatedAtAction("GetProductItem", new { id = productItem.Id }, productItem);
        }

        // DELETE: api/ProductQueue/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductItem(long id)
        {
            if (_context.ProductItems == null)
            {
                return NotFound();
            }
            var productItem = await _context.ProductItems.FindAsync(id);
            if (productItem == null)
            {
                return NotFound();
            }

            _context.ProductItems.Remove(productItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductItemExists(long id)
        {
            return (_context.ProductItems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

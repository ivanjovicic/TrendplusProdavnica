# Admin Panel Architecture

## 📋 Overview

Complete admin panel implementation for TrendplusProdavnica with:
- **JWT Authentication** - Secure token-based authentication
- **Role-Based Access Control** - Admin-only routes with middleware
- **Comprehensive Dashboard** - Overview of products, orders, and analytics
- **CRUD Operations** - For products, orders, brands, collections, editorial content, and homepage
- **Admin API** - RESTful endpoints for all admin operations

**Status**: ✅ **Ready for Integration**

## 🏗️ Architecture

### Backend Stack
- **.NET 10 / C#** - API implementation
- **PostgreSQL** - Data persistence
- **EF Core** - ORM
- **JWT** - Authentication tokens
- **Middleware** - Token validation and authorization

### Frontend Stack
- **Next.js 15** - App Router, Server/Client Components
- **React 19** - UI framework
- **TypeScript 5** - Type safety
- **Tailwind CSS 3.4** - Styling

### Authentication Flow
```
Client → Login Form
       ↓
API (POST /api/admin/auth/login)
       ↓
Server (Validate credentials)
       ↓
Generate JWT Token + Refresh Token
       ↓
Client stores in localStorage
       ↓
All future requests include "Authorization: Bearer {token}"
       ↓
Middleware validates token
       ↓
Routes protected via useAdmin() hook
```

## 📁 Project Structure

### Backend

```
TrendplusProdavnica.Application/
├── Admin/
│   ├── Models/
│   │   └── AuthModels.cs          # JWT claims, auth tokens
│   ├── Dtos/
│   │   └── AdminDtos.cs           # All admin DTOs
│   └── Services/
│       ├── IAdminServices.cs       # Service interfaces
│       └── JwtTokenService.cs      # Token generation

TrendplusProdavnica.Infrastructure/
└── Admin/
    └── Services/
        ├── AdminServices.cs        # Core service implementations
        └── AdminAuthService.cs     # Auth service impl

TrendplusProdavnica.Api/
├── Infrastructure/
│   └── Middleware/
│       └── JwtMiddleware.cs        # Token validation
├── Program.cs                      # JWT config + admin endpoints
└── appsettings.json                # JWT settings
```

### Frontend

```
src/
├── lib/
│   └── admin/
│       ├── client.ts               # API client
│       └── context.tsx             # Auth context + hooks
├── app/
│   ├── admin/
│   │   ├── login/
│   │   │   └── page.tsx            # Login page
│   │   ├── layout.tsx              # Admin layout + sidebar
│   │   ├── page.tsx                # Dashboard
│   │   ├── products/
│   │   │   └── page.tsx            # Products list
│   │   ├── orders/
│   │   │   └── page.tsx            # Orders list
│   │   ├── brands/
│   │   │   └── page.tsx            # Brands list
│   │   ├── collections/
│   │   │   └── page.tsx            # Collections list
│   │   ├── editorial/
│   │   │   └── page.tsx            # Editorial list
│   │   └── homepage/
│   │       └── page.tsx            # Homepage config
```

## 🔐 Security

### Authentication
- **JWT Tokens**: Secure tokens with expiration times
- **Refresh Tokens**: Long-lived tokens for obtaining new access tokens
- **Password Security**: Change before production (appsettings.json)
- **HTTPS Only**: All requests must use HTTPS in production

### Authorization
- **Admin Role Check**: Only users with "admin" role can access
- **Middleware Validation**: Every protected request validates token
- **Token Expiration**: Automatic refresh required after expiration
- **Logout Support**: Clear tokens from localStorage and server

### Configuration
```json
{
  "Jwt": {
    "SecretKey": "CHANGE_THIS_IN_PRODUCTION",
    "Issuer": "TrendplusProdavnica",
    "Audience": "TrendplusProdavnica.Admin",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "AdminAuth": {
    "Email": "admin@trendplus.com",
    "Password": "admin123!@#"
  }
}
```

## 📡 API Endpoints

### Authentication
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/admin/auth/login` | POST | Login with email/password |
| `/api/admin/auth/refresh` | POST | Refresh access token |

### Products
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/admin/products` | GET | List products (paginated) |
| `/api/admin/products/{id}` | GET | Get product details |
| `/api/admin/products` | POST | Create product |
| `/api/admin/products/{id}` | PUT | Update product |
| `/api/admin/products/{id}` | DELETE | Delete product |

### Orders
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/admin/orders` | GET | List orders (paginated) |
| `/api/admin/orders/{id}` | GET | Get order details |
| `/api/admin/orders/{id}/status` | PUT | Update order status |

### Brands
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/admin/brands` | GET | List brands (paginated) |

### Collections
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/admin/collections` | GET | List collections (paginated) |

### Editorial
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/admin/editorial` | GET | List articles (paginated) |

### Homepage
| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/admin/homepage` | GET | Get homepage configuration |

## 🎨 Admin UI Features

### Dashboard
- **Stats Cards**: Total products, orders, pending orders, active products
- **Recent Products**: Last 5 products with quick actions
- **Recent Orders**: Last 5 orders with status indicators
- **Real-time Data**: Loads on mount with error handling

### Products Management
- **List View**: Paginated products with search
- **Filters**: By name, brand, status
- **Actions**: Edit, delete with confirmation
- **Responsive**: Works on mobile, tablet, desktop

### Orders Management
- **List View**: Paginated orders with filters
- **Status Colors**: Visual indicators for order status
- **Customer Info**: Email, name, contact details
- **Actions**: View detail, update status

### Brands, Collections, Editorial, Homepage
- **Consistent UI**: Table-based lists with pagination
- **Easy Actions**: Edit, delete, toggle active/publish status
- **Filtering**: By status, publish state, etc.

## 🔑 Frontend Usage

### Login & Context
```tsx
// Login
const { login } = useAdmin();
await login("admin@trendplus.com", "admin123!@#");

// Use authentication state
const { user, token, isAuthenticated } = useAdmin();

// Logout
const { logout } = useAdmin();
logout();
```

### API Client
```tsx
// Get products
const response = await adminApiClient.getProducts(1, 20, "search");

// Create product
const product = await adminApiClient.createProduct({
  name: "New Product",
  price: 99.99,
  // ...
});

// Update product
const updated = await adminApiClient.updateProduct(id, {
  name: "Updated Name",
  // ...
});

// Delete product
await adminApiClient.deleteProduct(id);
```

## 📊 Admin Pages

### `/admin` - Dashboard
- Overview statistics
- Recent products and orders
- Quick insights

### `/admin/products`
- List all products
- Search functionality
- Create, edit, delete operations
- Stock quantity management

### `/admin/orders`
- List all orders
- Filter by status
- View order details
- Update order status

### `/admin/brands`
- List all brands
- Manage brand information
- Product count per brand

### `/admin/collections`
- List all collections
- Manage collection products
- Active/inactive toggle

### `/admin/editorial`
- List all articles
- Filter by publish status
- Create, edit, delete articles
- Featured article toggle

### `/admin/homepage`
- Visual section editor
- Add/remove sections
- Reorder sections
- Configure section settings

## 🛠️ Setup Instructions

### Backend Setup

1. **Add Migrations**:
   ```bash
   cd TrendplusProdavnica.Infrastructure
   dotnet ef migrations add AddAdminServices
   dotnet ef database update
   ```

2. **Update appsettings.json**:
   ```json
   {
     "Jwt": {
       "SecretKey": "generate-secure-key-32-chars-minimum",
       "Issuer": "TrendplusProdavnica",
       "Audience": "TrendplusProdavnica.Admin",
       "ExpirationMinutes": 60,
       "RefreshTokenExpirationDays": 7
     },
     "AdminAuth": {
       "Email": "your-admin-email",
       "Password": "secure-password-change-asap"
     }
   }
   ```

3. **Start API**:
   ```bash
   cd TrendplusProdavnica.Api
   dotnet run
   ```

### Frontend Setup

1. **Configure API URL** (`.env.local`):
   ```env
   NEXT_PUBLIC_API_URL=http://localhost:5000
   ```

2. **Wrap App with AdminProvider** (`app.tsx` or root layout):
   ```tsx
   import { AdminProvider } from "@/lib/admin/context";

   export default function RootLayout({ children }) {
     return (
       <html>
         <body>
           <AdminProvider>{children}</AdminProvider>
         </body>
       </html>
     );
   }
   ```

3. **Build and Run**:
   ```bash
   cd TrendplusProdavnica.Web
   npm run build
   npm start
   ```

## 🧪 Testing

### Login
```bash
Email: admin@trendplus.com
Password: admin123!@#
```

### API Testing
```bash
# Login
curl -X POST http://localhost:5000/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@trendplus.com","password":"admin123!@#"}'

# Get products (with token)
curl -X GET http://localhost:5000/api/admin/products \
  -H "Authorization: Bearer {token}"
```

## 📈 Scalability

### Future Enhancements
1. **Database Admin Users** - User management with roles
2. **Audit Logging** - Track all admin actions
3. **Bulk Operations** - Bulk edit/delete
4. **Export/Import** - CSV/Excel support
5. **Analytics Dashboard** - Advanced reporting
6. **Webhook Support** - External integrations
7. **Approval Workflow** - Multi-level approvals
8. **API Rate Limiting** - Prevent abuse

### Performance Optimizations
- Token caching (client-side)
- API response caching
- Pagination limits
- Database query optimization
- Image optimization in admin UI

## 🐛 Troubleshooting

### Token Not Working
- Check JWT secret key in appsettings.json
- Verify token expiration time (60 minutes default)
- Clear browser cache and refresh

### Admin Routes Not Protected
- Ensure AdminProvider wraps root layout
- Check useAdmin() hook authentication status
- Verify middleware is registered in Program.cs

### API Errors
- Check CORS configuration if frontend and backend on different ports
- Verify API is running on correct port
- Check appsettings.json configuration
- Review server logs for exception details

### Database Issues
- Run migrations: `dotnet ef database update`
- Check connection string in appsettings.json
- Verify PostgreSQL is running

## 📝 Version History

- **v1.0** (April 2026) - Initial implementation
  - JWT authentication
  - Admin dashboard
  - Products, Orders, Brands, Collections, Editorial, Homepage management
  - Responsive admin UI
  - Complete API endpoints

## 🔗 Related Documentation

- **PREMIUM_STOREFRONT_README.md** - Customer-facing storefront
- **OPENSEARCH_SYNC_PIPELINE_README.md** - Search indexing
- **STOREFRONT_ARCHITECTURE.md** - Frontend components
- **WEBSHOP_API_DOCUMENTATION.md** - Public API docs

---

**Built for TrendplusProdavnica**
Last Updated: April 2026

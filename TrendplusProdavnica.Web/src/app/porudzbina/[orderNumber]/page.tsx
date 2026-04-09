import { getOrder } from '@/lib/api/checkout';
import { formatPrice } from '@/lib/utils/helpers';
import { Metadata } from 'next';

interface OrderPageProps {
  params: { orderNumber: string };
}

export async function generateMetadata({
  params,
}: OrderPageProps): Promise<Metadata> {
  return {
    title: `Поредбина ${params.orderNumber}`,
    description: 'Детаљи вашей поредбине',
  };
}

export default async function OrderConfirmationPage({
  params,
}: OrderPageProps) {
  try {
    const order = await getOrder(params.orderNumber);

    if (!order) {
      return (
        <div className="min-h-screen bg-white flex items-center justify-center">
          <div className="text-center">
            <h1 className="text-2xl font-bold mb-4">Поредбина није пронађена</h1>
            <a href="/" className="text-blue-600 hover:underline">
              Назад на почетну
            </a>
          </div>
        </div>
      );
    }

    return (
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-4xl mx-auto px-4 py-8">
          {/* Success Banner */}
          <div className="bg-green-50 border border-green-200 rounded-lg p-8 mb-8 text-center">
            <div className="text-5xl mb-4">✓</div>
            <h1 className="text-3xl font-bold text-green-800 mb-2">
              Поредбина потврђена!
            </h1>
            <p className="text-gray-700 mb-4">
              Захвалю на вашој куповини. Редни број: <strong>{order.orderNumber}</strong>
            </p>
            <p className="text-sm text-gray-600">
              Потврда поредбине је послана на вашу email адресу
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Order Details */}
            <div className="lg:col-span-2 space-y-6">
              {/* Order Info */}
              <div className="bg-white rounded-lg p-6">
                <h2 className="text-2xl font-bold mb-6">Детаљи поредбине</h2>

                <div className="space-y-3 mb-6 border-b pb-6">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Редни број:</span>
                    <span className="font-semibold">{order.orderNumber}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Статус:</span>
                    <span className="font-semibold">{getPrettyStatus(order.status)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Датум:</span>
                    <span className="font-semibold">
                      {new Date(order.createdAt).toLocaleDateString('sr-RS')}
                    </span>
                  </div>
                </div>

                {/* Customer Info */}
                <h3 className="font-bold mb-3">Лични подаци</h3>
                <div className="text-sm text-gray-700 space-y-1 mb-6 border-b pb-6">
                  <p>{order.customerFullName}</p>
                  <p>{order.email}</p>
                  <p>{order.phone}</p>
                </div>

                {/* Delivery Address */}
                <h3 className="font-bold mb-3">Адреса за испоруку</h3>
                <div className="text-sm text-gray-700 space-y-1 mb-6 border-b pb-6">
                  <p>{order.deliveryAddressLine1}</p>
                  {order.deliveryAddressLine2 && <p>{order.deliveryAddressLine2}</p>}
                  <p>
                    {order.deliveryCity}, {order.deliveryPostalCode}
                  </p>
                </div>

                {/* Delivery & Payment Methods */}
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-gray-600 mb-1">Испорука</p>
                    <p className="font-semibold">
                      {getPrettyDeliveryMethod(order.deliveryMethod)}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600 mb-1">Плаћање</p>
                    <p className="font-semibold">
                      {getPrettyPaymentMethod(order.paymentMethod)}
                    </p>
                  </div>
                </div>
              </div>

              {/* Items */}
              <div className="bg-white rounded-lg p-6">
                <h2 className="text-2xl font-bold mb-6">Производи у поредбини</h2>

                <div className="space-y-4">
                  {order.items.map((item, i) => (
                    <div
                      key={i}
                      className="flex justify-between items-center border-b pb-4"
                    >
                      <div>
                        <p className="font-semibold">{item.productName}</p>
                        <p className="text-sm text-gray-600">
                          {item.brandName} • Величина: {item.sizeEu}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="font-semibold">
                          {item.quantity}x {formatPrice(item.unitPrice)}
                        </p>
                        <p className="text-sm text-gray-600">
                          Укупно: {formatPrice(item.lineTotal)}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Summary Sidebar */}
            <div className="lg:col-span-1">
              <div className="bg-white rounded-lg p-6 sticky top-4">
                <h2 className="text-2xl font-bold mb-6">Резиме плаћања</h2>

                <div className="space-y-3 mb-6 border-b pb-6">
                  <div className="flex justify-between">
                    <span>Производи:</span>
                    <span>{formatPrice(order.subtotalAmount)}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Испорука:</span>
                    <span>{formatPrice(order.deliveryAmount)}</span>
                  </div>
                </div>

                <div className="flex justify-between text-xl font-bold mb-8">
                  <span>Укупна цена:</span>
                  <span>{formatPrice(order.totalAmount)}</span>
                </div>

                {/* Info Box */}
                <div className="bg-blue-50 border border-blue-200 rounded p-4 text-sm text-blue-800 mb-6">
                  <p className="font-semibold mb-2">Шта је даље?</p>
                  <ul className="space-y-1 text-xs">
                    <li>✓ Потврда е-поште</li>
                    <li>✓ Припрема паковања</li>
                    <li>✓ Брзо слање</li>
                    <li>✓ Праћење доставе</li>
                  </ul>
                </div>

                <a
                  href="/"
                  className="block w-full bg-black text-white py-2 rounded text-center font-semibold hover:bg-gray-800"
                >
                  Назад на почетну
                </a>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  } catch (error) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold mb-4">Грешка при учитавању</h1>
          <a href="/" className="text-blue-600 hover:underline">
            Назад на почетну
          </a>
        </div>
      </div>
    );
  }
}

function getPrettyStatus(status: string) {
  const statuses: Record<string, string> = {
    Draft: 'Нацрт',
    PendingPayment: 'Чека се плаћање',
    Placed: 'Потврђена',
    Cancelled: 'Отказана',
  };
  return statuses[status] || status;
}

function getPrettyDeliveryMethod(method: string) {
  const methods: Record<string, string> = {
    Courier: 'Курир',
    StorePickup: 'Преузимање у продавници',
  };
  return methods[method] || method;
}

function getPrettyPaymentMethod(method: string) {
  const methods: Record<string, string> = {
    CashOnDelivery: 'Плаћање при доставци',
    CardPlaceholder: 'Картица',
  };
  return methods[method] || method;
}

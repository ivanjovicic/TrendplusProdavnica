import Link from 'next/link';

export interface StoreLocation {
  id: string;
  name: string;
  slug: string;
  address: string;
  city: string;
  phone?: string;
  email?: string;
  hours?: {
    monday?: string;
    tuesday?: string;
    wednesday?: string;
    thursday?: string;
    friday?: string;
    saturday?: string;
    sunday?: string;
  };
}

interface StoreLocatorProps {
  stores: StoreLocation[];
  layout?: 'grid' | 'list';
}

export function StoreLocator({ stores, layout = 'grid' }: StoreLocatorProps) {
  if (layout === 'list') {
    return (
      <div className="space-y-6">
        {stores.map((store) => (
          <StoreCard key={store.id} store={store} />
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {stores.map((store) => (
        <StoreCard key={store.id} store={store} />
      ))}
    </div>
  );
}

function StoreCard({ store }: { store: StoreLocation }) {
  return (
    <Link href={`/prodavnice/${store.slug}`}>
      <div className="border border-gray-200 p-6 group hover:border-gray-900 transition-colors">
        <h3 className="text-base font-medium text-gray-900 mb-4">
          {store.name}
        </h3>

        <div className="space-y-3 text-sm text-gray-600">
          {/* Address */}
          <div>
            <p className="font-medium text-gray-900">Lokacija</p>
            <p>{store.address}</p>
            <p>{store.city}</p>
          </div>

          {/* Contact */}
          {(store.phone || store.email) && (
            <div>
              <p className="font-medium text-gray-900">Kontakt</p>
              {store.phone && (
                <p>
                  <a href={`tel:${store.phone}`} className="hover:text-gray-900">
                    {store.phone}
                  </a>
                </p>
              )}
              {store.email && (
                <p>
                  <a href={`mailto:${store.email}`} className="hover:text-gray-900">
                    {store.email}
                  </a>
                </p>
              )}
            </div>
          )}

          {/* Hours */}
          {store.hours && Object.values(store.hours).some((h) => h) && (
            <div>
              <p className="font-medium text-gray-900">Radno vreme</p>
              <div className="space-y-1 text-xs">
                {store.hours.monday && <p>Ponedeljak: {store.hours.monday}</p>}
                {store.hours.tuesday && <p>Utorak: {store.hours.tuesday}</p>}
                {store.hours.wednesday && <p>Sreda: {store.hours.wednesday}</p>}
                {store.hours.thursday && <p>Četvrtak: {store.hours.thursday}</p>}
                {store.hours.friday && <p>Petak: {store.hours.friday}</p>}
                {store.hours.saturday && <p>Subota: {store.hours.saturday}</p>}
                {store.hours.sunday && <p>Nedelja: {store.hours.sunday}</p>}
              </div>
            </div>
          )}
        </div>

        <p className="text-xs tracking-wide text-gray-500 mt-4 pt-4 border-t border-gray-200 group-hover:text-gray-900">
          Pročitaj više
        </p>
      </div>
    </Link>
  );
}

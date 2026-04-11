'use client';

import { useState } from 'react';
import { AddToCartButton } from './add-to-cart';
import type { ProductdetailDto } from '@/lib/types';

interface ProductDetailsClientProps {
  product: ProductdetailDto;
}

export function ProductDetailsClient({ product }: ProductDetailsClientProps) {
  const [selectedVariantSku, setSelectedVariantSku] = useState<string | null>(null);
  const [showSizeError, setShowSizeError] = useState(false);

  // Extract variantId from SKU (format: "{variantId}-...")
  const selectedVariantId = selectedVariantSku ? Number(selectedVariantSku.split('-')[0]) : 0;

  const handleSizeSelect = (sku: string) => {
    setSelectedVariantSku(sku);
    setShowSizeError(false);
  };

  const handleAddToCartClick = () => {
    if (!selectedVariantSku) {
      setShowSizeError(true);
    }
  };

  return (
    <div>
      {product.sizes.length > 0 && (
        <div className="mb-6">
          <label className="mb-3 block text-sm font-semibold">Velicina</label>
          <div className="grid grid-cols-3 gap-2 sm:grid-cols-4 md:grid-cols-5">
            {product.sizes.map((size) => {
              const isDisabled = !size.isActive || !size.isVisible || size.totalStock === 0;
              const isSelected = selectedVariantSku === size.sku;

              return (
                <button
                  key={size.sku}
                  onClick={() => !isDisabled && handleSizeSelect(size.sku)}
                  disabled={isDisabled}
                  className={`rounded border py-2 px-1 text-sm font-medium transition-all ${
                    isSelected
                      ? 'border-black bg-black text-white'
                      : isDisabled
                        ? 'border-gray-300 bg-gray-100 text-gray-400 cursor-not-allowed'
                        : 'border-gray-300 hover:border-black'
                  }`}
                  title={isDisabled ? 'Nema dostupnih' : `${size.sizeEu} EU`}
                >
                  {size.sizeEu}
                </button>
              );
            })}
          </div>

          {showSizeError && (
            <p className="mt-2 text-sm text-red-600">Molimo izaberite veličinu prije nego što dodate u korpu.</p>
          )}
        </div>
      )}

      <AddToCartButton
        variantId={selectedVariantId}
        onSizeRequired={!selectedVariantSku && product.sizes.length > 0 ? handleAddToCartClick : undefined}
      />
    </div>
  );
}

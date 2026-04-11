import React from 'react';
import clsx from 'clsx';

interface HeroSectionProps {
  title: string;
  subtitle?: string;
  description?: string;
  cta?: {
    label: string;
    href: string;
  };
  align?: 'left' | 'center' | 'right';
  maxWidth?: 'sm' | 'md' | 'lg';
}

const maxWidthMap = {
  sm: 'max-w-md',
  md: 'max-w-2xl',
  lg: 'max-w-4xl',
};

export function HeroSection({
  title,
  subtitle,
  description,
  cta,
  align = 'center',
  maxWidth = 'lg',
}: HeroSectionProps) {
  const alignClasses = {
    left: 'text-left',
    center: 'text-center',
    right: 'text-right',
  };

  return (
    <section className="py-16 md:py-24 lg:py-32 bg-white">
      <div className="mx-auto px-4 sm:px-6 lg:px-8">
        <div
          className={clsx(
            'mx-auto',
            maxWidthMap[maxWidth],
            alignClasses[align]
          )}
        >
          {/* Subtitle / Eyebrow */}
          {subtitle && (
            <p className="text-xs tracking-widest text-gray-500 uppercase mb-4">
              {subtitle}
            </p>
          )}

          {/* Title - Premium Typography */}
          <h1 className="text-4xl md:text-5xl lg:text-6xl font-light leading-tight text-gray-900 mb-6">
            {title}
          </h1>

          {/* Description */}
          {description && (
            <p className="text-lg md:text-xl text-gray-600 mb-8 leading-relaxed">
              {description}
            </p>
          )}

          {/* CTA Button */}
          {cta && (
            <a
              href={cta.href}
              className="inline-block px-8 py-3 border border-gray-900 text-gray-900 hover:bg-gray-900 hover:text-white transition-colors duration-200 text-sm tracking-wide"
            >
              {cta.label}
            </a>
          )}
        </div>
      </div>
    </section>
  );
}

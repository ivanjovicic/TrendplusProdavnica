import React from 'react';
import clsx from 'clsx';

interface SectionProps {
  children: React.ReactNode;
  className?: string;
  spacingTop?: 'sm' | 'md' | 'lg' | 'xl';
  spacingBottom?: 'sm' | 'md' | 'lg' | 'xl';
  maxWidth?: 'sm' | 'md' | 'lg' | 'full';
}

const maxWidthMap = {
  sm: 'max-w-2xl',
  md: 'max-w-4xl',
  lg: 'max-w-6xl',
  full: 'max-w-full',
};

export function Section({
  children,
  className,
  spacingTop = 'md',
  spacingBottom = 'md',
  maxWidth = 'lg',
}: SectionProps) {
  return (
    <section
      className={clsx(
        'mx-auto px-4 sm:px-6 lg:px-8',
        spacingTop === 'sm' && 'pt-8 md:pt-12',
        spacingTop === 'md' && 'pt-12 md:pt-16',
        spacingTop === 'lg' && 'pt-16 md:pt-24',
        spacingTop === 'xl' && 'pt-20 md:pt-32',
        spacingBottom === 'sm' && 'pb-8 md:pb-12',
        spacingBottom === 'md' && 'pb-12 md:pb-16',
        spacingBottom === 'lg' && 'pb-16 md:pb-24',
        spacingBottom === 'xl' && 'pb-20 md:pb-32',
        maxWidthMap[maxWidth],
        className
      )}
    >
      {children}
    </section>
  );
}

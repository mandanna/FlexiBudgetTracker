export function currencyFormatter(amount: number): string {

    
    const options: Intl.NumberFormatOptions = {
        style: 'currency',
        currency: 'USD',
    };
    return new Intl.NumberFormat(undefined, options).format(amount);


}

/**
 * Formula Evaluator
 * Evaluates formulas with form data and calculates results
 */

const FormulaEvaluator = {
    /**
     * Evaluate a formula with given data
     * @param {string} formula - Formula expression
     * @param {object} data - Field values { field_name: value }
     * @returns {object} - Evaluation result { value, error }
     */
    evaluate(formula, data = {}) {
        if (!formula || formula.trim() === '') {
            return { value: null, error: 'Formula is empty' };
        }

        try {
            // Replace field references with actual values
            let expression = formula;

            // Extract all field references
            const fieldPattern = /\b([a-z_][a-z0-9_-]*)\b/gi;
            const fields = new Set();
            let match;

            while ((match = fieldPattern.exec(formula)) !== null) {
                const field = match[1].toLowerCase();
                if (!this.isReservedWord(field)) {
                    fields.add(field);
                }
            }

            // Replace fields with values (with null safety)
            fields.forEach(field => {
                const value = data[field];
                const numValue = this.toNumber(value);

                // Replace field references with numeric values
                const fieldRegex = new RegExp(`\\b${field}\\b`, 'gi');
                expression = expression.replace(fieldRegex, numValue.toString());
            });

            // Handle functions
            expression = this.evaluateFunctions(expression, data);

            // Evaluate the expression
            const result = this.safeEval(expression);

            return {
                value: result,
                error: null
            };
        } catch (error) {
            return {
                value: null,
                error: error.message
            };
        }
    },

    /**
     * Safely evaluate mathematical expression
     * @param {string} expression - Math expression
     * @returns {number} - Result
     */
    safeEval(expression) {
        // Remove whitespace
        expression = expression.trim();

        // Allowed characters: digits, operators, parentheses, decimal point
        if (!/^[0-9+\-*/(). ]+$/.test(expression)) {
            throw new Error('Invalid expression');
        }

        try {
            // Use Function constructor for safer eval
            const result = new Function('return ' + expression)();

            if (typeof result !== 'number' || !isFinite(result)) {
                throw new Error('Invalid calculation result');
            }

            return result;
        } catch (error) {
            throw new Error('Calculation error: ' + error.message);
        }
    },

    /**
     * Convert value to number with null safety
     * @param {any} value - Value to convert
     * @returns {number} - Numeric value or 0
     */
    toNumber(value) {
        if (value === null || value === undefined || value === '') {
            return 0;
        }

        const num = parseFloat(value);
        return isNaN(num) ? 0 : num;
    },

    /**
     * Evaluate function calls in expression
     * @param {string} expression - Expression with functions
     * @param {object} data - Field values
     * @returns {string} - Expression with functions evaluated
     */
    evaluateFunctions(expression, data) {
        // SUM function
        expression = expression.replace(/SUM\s*\((.*?)\)/gi, (match, args) => {
            const fields = args.split(',').map(f => f.trim());
            const sum = fields.reduce((acc, field) => acc + this.toNumber(data[field]), 0);
            return sum.toString();
        });

        // AVG function
        expression = expression.replace(/AVG\s*\((.*?)\)/gi, (match, args) => {
            const fields = args.split(',').map(f => f.trim());
            const sum = fields.reduce((acc, field) => acc + this.toNumber(data[field]), 0);
            const avg = fields.length > 0 ? sum / fields.length : 0;
            return avg.toString();
        });

        // MIN function
        expression = expression.replace(/MIN\s*\((.*?)\)/gi, (match, args) => {
            const fields = args.split(',').map(f => f.trim());
            const values = fields.map(field => this.toNumber(data[field]));
            const min = Math.min(...values);
            return min.toString();
        });

        // MAX function
        expression = expression.replace(/MAX\s*\((.*?)\)/gi, (match, args) => {
            const fields = args.split(',').map(f => f.trim());
            const values = fields.map(field => this.toNumber(data[field]));
            const max = Math.max(...values);
            return max.toString();
        });

        // COUNT function
        expression = expression.replace(/COUNT\s*\((.*?)\)/gi, (match, args) => {
            const fields = args.split(',').map(f => f.trim());
            const count = fields.filter(field => {
                const val = data[field];
                return val !== null && val !== undefined && val !== '';
            }).length;
            return count.toString();
        });

        // ROUND function
        expression = expression.replace(/ROUND\s*\(([^,]+),\s*(\d+)\)/gi, (match, value, decimals) => {
            const num = this.toNumber(value);
            const dec = parseInt(decimals);
            const rounded = Math.round(num * Math.pow(10, dec)) / Math.pow(10, dec);
            return rounded.toString();
        });

        // ABS function
        expression = expression.replace(/ABS\s*\((.*?)\)/gi, (match, value) => {
            return Math.abs(this.toNumber(value)).toString();
        });

        // CEIL function
        expression = expression.replace(/CEIL\s*\((.*?)\)/gi, (match, value) => {
            return Math.ceil(this.toNumber(value)).toString();
        });

        // FLOOR function
        expression = expression.replace(/FLOOR\s*\((.*?)\)/gi, (match, value) => {
            return Math.floor(this.toNumber(value)).toString();
        });

        return expression;
    },

    /**
     * Check if word is reserved
     * @param {string} word - Word to check
     * @returns {boolean} - True if reserved
     */
    isReservedWord(word) {
        const reserved = [
            'sum', 'avg', 'min', 'max', 'count', 'round', 'abs', 'ceil', 'floor',
            'and', 'or', 'not', 'true', 'false', 'null', 'pi', 'e'
        ];
        return reserved.includes(word.toLowerCase());
    },

    /**
     * Format number for display
     * @param {number} value - Numeric value
     * @param {number} decimals - Decimal places
     * @returns {string} - Formatted string
     */
    format(value, decimals = 2) {
        if (value === null || value === undefined) {
            return '';
        }

        if (typeof value !== 'number') {
            value = this.toNumber(value);
        }

        return value.toFixed(decimals);
    },

    /**
     * Evaluate formula for table totals/aggregates
     * @param {array} rows - Table row data
     * @param {string} columnName - Column to aggregate
     * @param {string} aggregateType - SUM, AVG, MIN, MAX, COUNT
     * @returns {number} - Aggregate result
     */
    evaluateAggregate(rows, columnName, aggregateType) {
        const values = rows.map(row => this.toNumber(row[columnName]));

        switch (aggregateType.toUpperCase()) {
            case 'SUM':
                return values.reduce((acc, val) => acc + val, 0);
            case 'AVG':
                return values.length > 0 ? values.reduce((acc, val) => acc + val, 0) / values.length : 0;
            case 'MIN':
                return values.length > 0 ? Math.min(...values) : 0;
            case 'MAX':
                return values.length > 0 ? Math.max(...values) : 0;
            case 'COUNT':
                return values.filter(v => v !== 0).length;
            default:
                return 0;
        }
    }
};

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = FormulaEvaluator;
}

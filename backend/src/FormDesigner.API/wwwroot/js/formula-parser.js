/**
 * Formula Parser
 * Parses and validates formula expressions for computed fields
 */

const FormulaParser = {
    /**
     * Parse a formula expression and extract field references
     * @param {string} formula - Formula expression (e.g., "quantity * unit_price")
     * @returns {object} - Parsed formula with dependencies
     */
    parse(formula) {
        if (!formula || formula.trim() === '') {
            return { valid: false, error: 'Formula is empty' };
        }

        try {
            // Extract field references (alphanumeric with underscores/hyphens)
            const fieldPattern = /\b([a-z_][a-z0-9_-]*)\b/gi;
            const fields = [];
            let match;

            while ((match = fieldPattern.exec(formula)) !== null) {
                const field = match[1].toLowerCase();
                // Skip operators and functions
                if (!this.isReservedWord(field)) {
                    if (!fields.includes(field)) {
                        fields.push(field);
                    }
                }
            }

            // Validate formula syntax
            const validationResult = this.validateSyntax(formula);
            if (!validationResult.valid) {
                return validationResult;
            }

            return {
                valid: true,
                formula: formula,
                dependencies: fields,
                operators: this.extractOperators(formula),
                functions: this.extractFunctions(formula)
            };
        } catch (error) {
            return {
                valid: false,
                error: error.message
            };
        }
    },

    /**
     * Validate formula syntax
     * @param {string} formula - Formula expression
     * @returns {object} - Validation result
     */
    validateSyntax(formula) {
        // Check for balanced parentheses
        let depth = 0;
        for (const char of formula) {
            if (char === '(') depth++;
            if (char === ')') depth--;
            if (depth < 0) {
                return { valid: false, error: 'Unbalanced parentheses' };
            }
        }
        if (depth !== 0) {
            return { valid: false, error: 'Unbalanced parentheses' };
        }

        // Check for invalid characters
        const validPattern = /^[a-z0-9_\-+\-*/(). ,]+$/i;
        if (!validPattern.test(formula)) {
            return { valid: false, error: 'Invalid characters in formula' };
        }

        // Check for double operators
        if (/[+\-*\/]{2,}/.test(formula.replace('**', ''))) {
            return { valid: false, error: 'Invalid operator sequence' };
        }

        return { valid: true };
    },

    /**
     * Extract operators from formula
     * @param {string} formula - Formula expression
     * @returns {array} - List of operators used
     */
    extractOperators(formula) {
        const operators = [];
        const opPattern = /[+\-*\/]/g;
        let match;

        while ((match = opPattern.exec(formula)) !== null) {
            if (!operators.includes(match[0])) {
                operators.push(match[0]);
            }
        }

        return operators;
    },

    /**
     * Extract function calls from formula
     * @param {string} formula - Formula expression
     * @returns {array} - List of functions used
     */
    extractFunctions(formula) {
        const functions = [];
        const funcPattern = /\b(SUM|AVG|MIN|MAX|COUNT|ROUND|ABS|CEIL|FLOOR)\s*\(/gi;
        let match;

        while ((match = funcPattern.exec(formula)) !== null) {
            const func = match[1].toUpperCase();
            if (!functions.includes(func)) {
                functions.push(func);
            }
        }

        return functions;
    },

    /**
     * Check if word is a reserved keyword or function
     * @param {string} word - Word to check
     * @returns {boolean} - True if reserved
     */
    isReservedWord(word) {
        const reserved = [
            // Operators
            'and', 'or', 'not',
            // Functions
            'sum', 'avg', 'min', 'max', 'count', 'round', 'abs', 'ceil', 'floor',
            // Constants
            'true', 'false', 'null', 'pi', 'e'
        ];

        return reserved.includes(word.toLowerCase());
    },

    /**
     * Get human-readable formula description
     * @param {object} parsed - Parsed formula result
     * @returns {string} - Description
     */
    describe(parsed) {
        if (!parsed.valid) {
            return `Invalid: ${parsed.error}`;
        }

        let desc = `Formula uses ${parsed.dependencies.length} field(s)`;
        if (parsed.dependencies.length > 0) {
            desc += `: ${parsed.dependencies.join(', ')}`;
        }

        if (parsed.functions.length > 0) {
            desc += ` | Functions: ${parsed.functions.join(', ')}`;
        }

        return desc;
    }
};

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = FormulaParser;
}

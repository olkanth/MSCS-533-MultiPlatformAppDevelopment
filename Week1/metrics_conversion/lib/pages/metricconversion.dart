/*
 * File: metricconversion.dart
 * Description: Contains the main logic and UI for the Metric Conversion page.
 * Implements a StatefulWidget to handle user input and dynamic updates.
 */

import 'package:flutter/material.dart';

class MetricConversion extends StatefulWidget {
  const MetricConversion({super.key});

  @override
  State<MetricConversion> createState() => _MetricConversionState();
}

class _MetricConversionState extends State<MetricConversion> {
  final TextEditingController _inputController = TextEditingController();
  String _fromUnit = 'Meters';
  String _toUnit = 'Kilometers';
  String _result = '';

  // Dictionary to store conversion rates
  final Map<String, double> _conversionRates = {
    'Meters': 1.0,
    'Kilometers': 1000.0,
    'Feet': 0.3048,
    'Miles': 1609.34,
    'Inches': 0.0254,
    'Centimeters': 0.01,
    'Yards': 0.9144,
  };
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Measures Converter"),
        centerTitle: true,
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      ),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            Text(
              "Value",
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w400)
            ),
            const SizedBox(height: 20),
            TextField(
              controller: _inputController,
              keyboardType: TextInputType.number,
              decoration: const InputDecoration(
                labelText: 'Enter value',
                border: OutlineInputBorder(),
              ),
            ),

            const SizedBox(height: 20),
            Text(
              "From",
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w400)
            ),
            const SizedBox(height: 20),
            DropdownButton(
              items: _conversionRates.keys.map<DropdownMenuItem<String>>((
                String value,
              ) {
                return DropdownMenuItem<String>(
                  value: value,
                  child: Text(value),
                );
              }).toList(),
              onChanged: (String? value) {
                setState(() {
                  _fromUnit = value!;
                });
              },
              value: _fromUnit,
              isExpanded: true,
              hint: const Text('From Unit'),
            ),
            const SizedBox(height: 20),
            Text(
              "To",
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w400)
            ),
            DropdownButton(
              items: _conversionRates.keys.map<DropdownMenuItem<String>>((
                String value,
              ) {
                return DropdownMenuItem<String>(
                  value: value,
                  child: Text(value),
                );
              }).toList(),
              onChanged: (String? value) {
                setState(() {
                  _toUnit = value!;
                });
              },
              value: _toUnit,
              isExpanded: true,
              hint: const Text('To Unit'),
            ),
            const SizedBox(height: 20),
            ElevatedButton(
              onPressed: _convertValue,
              style: ElevatedButton.styleFrom(
                backgroundColor: Theme.of(context).colorScheme.primary,
                foregroundColor: Theme.of(context).colorScheme.onPrimary,
              ),
              child: const Text('Convert'),
            ),
            const SizedBox(height: 20),
            Text(_result, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
          ],
        ),
      ),
    );
  }

  // Function to handle the conversion logic
  void _convertValue() {
    double? input = double.tryParse(_inputController.text);
    if (input == null) {
      setState(() {
        _result = 'Please enter a input value';
      });
      return;
    }

    // Perform the conversion based on the selected units
    double fromRate = _conversionRates[_fromUnit]!;
    double toRate = _conversionRates[_toUnit]!;
    double resultValue = (input * fromRate) / toRate;

  // Update the result text
    setState(() {
      _result =
          '$input $_fromUnit are ${resultValue.toStringAsFixed(4)} $_toUnit';
    });
  }
}
